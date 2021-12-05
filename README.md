# About
A minimalistic MVVM framework for the new Unity UI Toolkit build on [https://github.com/EcsRx/ecsrx](EcsRx).

[![Build Status](https://github.com/Cosmic-Shores/EcsRx.Plugins.UnityUx/actions/workflows/publish.yml/badge.svg)](https://github.com/Cosmic-Shores/EcsRx.Plugins.UnityUx/actions)
[![openupm](https://img.shields.io/npm/v/com.ecsrx.plugins.unityux?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.ecsrx.plugins.unityux/)
[![License](https://badgen.net/github/license/Naereen/Strapdown.js)](https://github.com/Cosmic-Shores/EcsRx.Plugins.UnityUx/blob/main/LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com)

## Requirements / Versions
### Since mayor version 2: [![Unity 2021.2+](https://img.shields.io/badge/unity-2021.2%2B-blue.svg)](https://unity3d.com/get-unity/download)
_About_: Uses **Rx.Unity** and **System.Reactive** instead of UniRx.
You need Unity 2021.2 or newer.

Your project must contain the following libraries somewhere:
- Serilog.dll (>= 2.0.0; eg. from NuGet)
- System.Reactive.dll (>= 5.0.0; eg. from NuGet*)
- Rx.Extendibility.dll (>= 1.0.2; eg. from NuGet*)
- Rx.Data.dll (>= 1.0.2; eg. from NuGet*)
- SystemsRx.dll (>= 5.1.0; eg. from NuGet)
- SystemsRx.Infrastructure.dll (>= 5.1.0; eg. from NuGet)

\* needs to be build from source and as System.Reactive still needs a minor change that hasn't been integrated yet. Since System.Reactive is also signed Rx.Extendibility and Rx.Data also need to be recompiled to work with an unsigned version of System.Reactive. You can aquire the DLLs by checking out [Rx.Unity](https://github.com/Cosmic-Shores/Rx.Unity) and running `./build-dependencies.bat` in the repository root (you need docker with buildkit for this). After this the required DLLs can be found in `./Dependencies/out/`.

Openupm dependencies:
- UPM: com.unity.modules.uielements (>= 1.0.0; Official UPM Package; Name: UI Toolkit)
- Rx.Unity.dll (>= 1.0.0; openupm: com.rx.unity)

### For mayor version 1: [![Unity 2021.1+](https://img.shields.io/badge/unity-2021.1%2B-blue.svg)](https://unity3d.com/get-unity/download)
_About_: Uses **UniRx**
You need a non LTS version of Unity 2021.1 or newer (older versions might not work).

- UPM: com.unity.modules.uielements (>= 1.0.0-preview.17; Official UPM Package; Name: UI Toolkit)
- Serilog.dll (>= 2.0.0; eg. from NuGet)
- UniRx.dll (>= 7.1.0; eg. from the asset store or from openupm: com.neuecc.unirx)
- SystemsRx.dll (>= 5.1.0; eg. from NuGet)
- SystemsRx.Infrastructure.dll (>= 5.1.0; eg. from NuGet)

## Installation
You have the following options:
1. The package is available on the [openupm registry](https://openupm.com). You can install it via [openupm-cli](https://github.com/openupm/openupm-cli). (recommended)
```
openupm add com.ecsrx.plugins.unityux
```
2. You can also install via git url by adding this entry in your **manifest.json**
```
"com.ecsrx.plugins.unityux": "https://github.com/Cosmic-Shores/EcsRx.Plugins.UnityUx.git#upm"
```

### Usage
To use this plugin you have to load both these plugins in your application.
- UnityUxPlugin

If you are using version 2 you will have to replace the **UniRx** using with a couple different ones but that should be fairly staight forward.

The following snippet ilustrates how a mvvm binding can be created.

#### UXML View
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xsi="http://www.w3.org/2001/XMLSchema-instance" engine="UnityEngine.UIElements" noNamespaceSchemaLocation="../../../../UIElementsSchema/UIElements.xsd" editor-extension-mode="False">
    <ui:VisualElement name="Settings">
        <ui:ScrollView name="SettingsContent">
            <ui:Foldout text="General">
                <ui:TextField label="Player name" name="PlayerName" value="" />
                <ui:SliderInt label="Auto save after turns" name="AutoSaveAfterTurns" low-value="0" high-value="10" page-size="1" value="1" />
                <ui:TextField label=" " class="slider-value" name="AutoSaveAfterTurnsLabel" value="1" />
                <ui:Toggle label="Discord rich experience" name="EnableDiscordRichExperience" />
                <ui:Toggle label="Provide logs to the developer" name="AllowLogUpload" />
            </ui:Foldout>
            <ui:Foldout text="Audio">
                <ui:Slider label="Master Volume" name="MasterVolume" low-value="0" high-value="100" page-size="1" value="100" />
                <ui:TextField label=" " class="slider-value" name="MasterVolumeLabel" value="100%" />
                <ui:Slider label="Music Volume" name="MusicVolume" low-value="0" high-value="100" page-size="1" value="100" />
                <ui:TextField label=" " class="slider-value" name="MusicVolumeLabel" value="100%" />
                <ui:Slider label="SFX Volume" name="SfxVolume" low-value="0" high-value="100" page-size="1" value="100" />
                <ui:TextField label=" " class="slider-value" name="SfxVolumeLabel" value="100%" />
            </ui:Foldout>
        </ui:ScrollView>
        <ui:VisualElement class="button-row">
            <ui:Button text="Save" name="Save" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

#### UxComponent
```cs
using EcsRx.Plugins.UnityUx;
using System;
using UniRx;

sealed class SettingsComponent : IUxComponent {
    public IReactiveProperty<string> PlayerName { get; set; }
    public IReactiveProperty<int> AutoSaveAfterTurns { get; set; }
    public IReactiveProperty<bool> AllowLogUpload { get; set; }
    public IReactiveProperty<float> MasterVolume { get; set; }
    public IReactiveProperty<float> MusicVolume { get; set; }
    public IReactiveProperty<float> SfxVolume { get; set; }
    public ISubject<Unit> Save { get; } = new Subject<Unit>();
}
```

#### UxBinder
Your `IUxBinder` ties the elements from the binding to their models properties they should be syncronized with.
This has some boilerplate code but the provided extensions most cases very simplistic and it's very flexible.

```cs
using EcsRx.Plugins.UnityUx;
using UniRx;
using UnityEngine.UIElements;

sealed class SettingsBinder : IUxBinder<SettingsComponent> {
    public VisualElement CreateBoundView(SettingsComponent component, UxContext context) {
        VisualTreeAsset templateAsset; // load the view template for example by using unity addressables
        var element = templateAsset.CloneTree();

        element.Q<TextField>("PlayerName").BindValue2Way(component.PlayerName, context);
        element.Q<SliderInt>("AutoSaveAfterTurns").BindValue2Way(component.AutoSaveAfterTurns, context);
        element.Q<TextField>($"AutoSaveAfterTurnsLabel").BindValue(component.AutoSaveAfterTurns.Select(GetAutoSaveDisplayText).TakeUntil(context));
        element.Q<Toggle>("AllowLogUpload").BindValue2Way(component.AllowLogUpload, context);

        BindVolumeSlider(element, "MasterVolume", component.MasterVolume, context);
        BindVolumeSlider(element, "MusicVolume", component.MusicVolume, context);
        BindVolumeSlider(element, "SfxVolume", component.SfxVolume, context);

        element.Q("Save").Click().BindTo(component.Save, context);
        return element;
    }

    private string GetAutoSaveDisplayText(int autoSaveAfterTurns) => autoSaveAfterTurns switch {
        0 => "Never",
        1 => "Every turn",
        _ => $"Every {autoSaveAfterTurns} turns",
    };

    private static void BindVolumeSlider(TemplateContainer element, string name, IReactiveProperty<float> rxProperty, UxContext context) {
        element.Q<Slider>(name).BindValue2Way(rxProperty, context);
        element.Q<TextField>($"{name}Label").BindValue(rxProperty.Select(value => $"{value:F1}%").TakeUntil(context));
    }
}
```

#### Final-Usage / Presenter & Module
With the `IUxBindingService` you can later make use of your `SettingsComponent` and the view binding that you have empowered it with.

```cs
using EcsRx.Groups;
using EcsRx.Groups.Observable;
using EcsRx.Plugins.GroupBinding.Attributes;
using EcsRx.Plugins.UnityUx;
using Serilog;
using System;
using SystemsRx.Infrastructure.Dependencies;
using SystemsRx.Infrastructure.Extensions;
using UniRx;

sealed class RootPresenter : ICustomGroupSystem {
    private readonly ISubject<Unit> _destroy = new Subject<Unit>();
    private readonly IUxBindingService _uxBindingService;
    private readonly ILogger _logger;

    // MyUxRoot would be something you'd have to have in the project already - see EcsRx docs for reference
    public IGroup Group { get; } = new Group(typeof(MyUxRoot));
    [FromGroup]
    public IObservableGroup ObservableGroup { get; set; }

    public RootPresenter(IUxBindingService uxBindingService, ILogger logger) {
        _uxBindingService = uxBindingService;
        _logger = logger.ForContext<RootPresenter>();
    }

    public void StartSystem() {
        Observable.EveryFixedUpdate().TakeUntil(_destroy).Skip(5).Take(1).Subscribe(OnInit);
    }

    public void StopSystem() => _destroy.OnNext(Unit.Default);

    private void OnInit(long _) {
        var notifications = new ReactiveCollection<NotificationEntry>();
        var component = new SettingsComponent {
            PlayerName = new ReactiveProperty<string>("Example"),
            AllowLogUpload = new ReactiveProperty<bool>(true),
            AutoSaveAfterTurns = new ReactiveProperty<int>(5),
            MasterVolume = new ReactiveProperty<float>(43.23),
            MusicVolume = new ReactiveProperty<float>(54.34),
            SfxVolume = new ReactiveProperty<float>(12.34)
        };

        // handle relevant changes
        component.Save.TakeUntil(destroy).Subscribe(_ => Save(component));
        
        VisualElement interactableRoot; // container to add dynamic ui onto
        _uxBindingService.PopulateChild(UxContext.CreateRootContext(_destroy, _logger), interactableRoot, component);
    }

    private void Save(SettingsComponent settings) {
        // insert save code here
    }
}

// remember to load this module as well
sealed class MyModule : IDependencyModule {
    public void Setup(IDependencyContainer container) {
        container.Bind<ISystem, SettingsBinder>();
        container.Bind<ISystem, RootPresenter>();
    }
}
```

#### Scope of possibilities
Apart from the simplest method in the `IUxBindingService` shown above it also supports more complicated scenarios like an `IUxComponent` embed inside of an IObservable or even a changing list by using `IReadOnlyReactiveCollection<IUxComponent>`.

The whole idea behind this is to also use the `IUxBindingService` inside of your IUxBinder if necessary to be able to bind nested `IUxComponent` structures.


## Roadmap / future thoughts
### Step 1: IUxViewModel & Factory
- add something like IUxViewModel which can optionally be used on top of the IUxComponent to properly seperate pure UI bindings and the view provided for others to be used.
- as this might result in way too many classes for simple cases make this (IUxViewModel) completely optional. Maybe make it possible for the IUxComponent to be the IUxViewModel too in which case a default factory will just return the same instance. (see next point)
- add some kind of a factory interface to create an IUxComponent from an IUxViewModel (and do that internally in the framework if a IUxViewModel is passed insetad of an IUxComponent) - The resulting factory would end up being something like a controller/ component in angular. Make come up with some kind of convention to maybe nest the factory inside of the IUxViewModel (the factory might end up being internal but the IUxViewModel public - hence why can't do it switched; they basically belong together and the number of class required for this framework is quite high - hence why this might make it a little easier to work)

### Step 2: getting rid of simple IUxBinders
- by doing the things in _step 1_ the thought of trying to streamline IUxBinders becomes alot easier to imagine.
- instead of using an IUxBinder one should also have the option to just decorate a IUxComponent with attributes and handle common bindings that way.
- attributes on properties can be bound by a corresponding IUxBindingHandler in a similar fashion as it's done in knockout.js. (IUxBindingHandler would be a new thing as well ofc)
- attributes on the IUxComponent class itself can be used to reference the uxml asset to be used in some way.
- there should be an IUxTemplateProvider that can have different implementations to handle different use cases of getting hold of/ loading/ resolving the uxml asset. (one of them would be by using addressables if an addresaddress is specified in the attribute)
