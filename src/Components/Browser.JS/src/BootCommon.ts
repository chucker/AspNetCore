import { SimpleEventDispatcher, SignalDispatcher } from "strongly-typed-events";

export async function fetchBootConfigAsync() {
  // Later we might make the location of this configurable (e.g., as an attribute on the <script>
  // element that's importing this file), but currently there isn't a use case for that.
  const bootConfigResponse = await fetch('_framework/blazor.boot.json', { method: 'Get', credentials: 'include' });
  return bootConfigResponse.json() as Promise<BootJsonData>;
}

export function loadEmbeddedResourcesAsync(bootConfig: BootJsonData): Promise<any> {
  const cssLoadingPromises = bootConfig.cssReferences.map(cssReference => {
    const linkElement = document.createElement('link');
    linkElement.rel = 'stylesheet';
    linkElement.href = cssReference;
    return loadResourceFromElement(linkElement);
  });
  const jsLoadingPromises = bootConfig.jsReferences.map(jsReference => {
    const scriptElement = document.createElement('script');
    scriptElement.src = jsReference;
    return loadResourceFromElement(scriptElement);
  });
  return Promise.all(cssLoadingPromises.concat(jsLoadingPromises));
}

function loadResourceFromElement(element: HTMLElement) {
  return new Promise((resolve, reject) => {
    element.onload = resolve;
    element.onerror = reject;
    document.head!.appendChild(element);
  });
}

// Keep in sync with BootJsonData in Microsoft.AspNetCore.Blazor.Build
interface BootJsonData {
  main: string;
  entryPoint: string;
  assemblyReferences: string[];
  cssReferences: string[];
  jsReferences: string[];
  linkerEnabled: boolean;
}

// Tells you if the script was added without <script src="..." autostart="false"></script>
export function shouldAutoStart() {
  return document &&
    document.currentScript &&
    document.currentScript.getAttribute('autostart') !== 'false';
}

export class BlazorBootProgress {
    private _onBootProgress = new SimpleEventDispatcher<[number, number]>();
    private _onBootCompletion = new SignalDispatcher();
    private _onBootFailure = new SignalDispatcher();

    public dispatchBootProgress(args: [number, number]) {
        //if (args[1] == 0)
        //    return;

        console.log(args[0]);
        console.log(args[1]);

        var blazorBootPercentage = document.getElementById('blazorBootPercentage');

        if (blazorBootPercentage)
            //blazorBootPercentage.innerText = ((args[0] / args[1]) * 100).toString();
            blazorBootPercentage.innerHTML = args[0].toString() + " / " + args[1].toString();

        this._onBootProgress.dispatch(args);
    }

    public onBootProgress() {
        return this._onBootProgress.asEvent();
    }

    public onBootCompletion() {
        return this._onBootCompletion.asEvent();
    }
}

//window['BlazorBoot'] = {
//    private 
//}
