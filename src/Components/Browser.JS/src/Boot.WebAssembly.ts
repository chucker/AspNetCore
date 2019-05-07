import '@dotnet/jsinterop';
import './GlobalExports';
import * as Environment from './Environment';
import { monoPlatform } from './Platform/Mono/MonoPlatform';
import { getAssemblyNameFromUrl } from './Platform/Url';
import { renderBatch } from './Rendering/Renderer';
import { SharedMemoryRenderBatch } from './Rendering/RenderBatch/SharedMemoryRenderBatch';
import { Pointer } from './Platform/Platform';
import { fetchBootConfigAsync, loadEmbeddedResourcesAsync, shouldAutoStart, BlazorBootProgress } from './BootCommon';

let started = false;

var bootProgress = new BlazorBootProgress();

async function boot(options?: any) {
    if (started) {
        throw new Error('Blazor has already started.');
    }
    started = true;

    // Configure environment for execution under Mono WebAssembly with shared-memory rendering
    const platform = Environment.setPlatform(monoPlatform);
    window['Blazor'].platform = platform;
    window['Blazor']._internal.renderBatch = (browserRendererId: number, batchAddress: Pointer) => {
        renderBatch(browserRendererId, new SharedMemoryRenderBatch(batchAddress));
    };

    // Fetch the boot JSON file
    const bootConfig = await fetchBootConfigAsync();

    var totalResources = bootConfig.assemblyReferences.length +
        bootConfig.cssReferences.length +
        bootConfig.jsReferences.length +
        1; // entryPoint

    bootProgress.dispatchBootProgress([0, totalResources]);

    const embeddedResourcesPromise = loadEmbeddedResourcesAsync(bootConfig);

    bootProgress.dispatchBootProgress([0 + bootConfig.cssReferences.length + bootConfig.jsReferences.length,
        totalResources]);

    if (!bootConfig.linkerEnabled) {
        console.info('Blazor is running in dev mode without IL stripping. To make the bundle size significantly smaller, publish the application or see https://go.microsoft.com/fwlink/?linkid=870414');
    }

    // Determine the URLs of the assemblies we want to load, then begin fetching them all
    const loadAssemblyUrls = [bootConfig.main]
        .concat(bootConfig.assemblyReferences)
        .map(filename => `_framework/_bin/${filename}`);

    try {
        await platform.start(loadAssemblyUrls);
    } catch (ex) {
        throw new Error(`Failed to start platform. Reason: ${ex}`);
    }

    // Before we start running .NET code, be sure embedded content resources are all loaded
    await embeddedResourcesPromise;

    // Start up the application
    const mainAssemblyName = getAssemblyNameFromUrl(bootConfig.main);
    platform.callEntryPoint(mainAssemblyName, bootConfig.entryPoint, []);
}

window['Blazor'].start = boot;

window['Blazor'].bootProgress = bootProgress;

if (shouldAutoStart()) {
    boot();
}
