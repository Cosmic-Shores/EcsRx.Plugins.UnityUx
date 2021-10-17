# About
A minimalistic MVVM framework for the new Unity UI Toolkit build on [https://github.com/EcsRx/ecsrx](EcsRx).

[![Unity 2021.1+](https://img.shields.io/badge/unity-2021.1%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![Build Status](https://github.com/Cosmic-Shores/EcsRx.Plugins.UnityUx/actions/workflows/publish.yml/badge.svg)](https://github.com/Cosmic-Shores/EcsRx.Plugins.UnityUx/actions)
[![openupm](https://img.shields.io/npm/v/com.ecsrx.plugins.unityux?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.ecsrx.plugins.unityux/)
[![License](https://badgen.net/github/license/Naereen/Strapdown.js)](https://github.com/Cosmic-Shores/EcsRx.Plugins.UnityUx/blob/main/LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com)

## Requirements
You need an non LTS version of Unity 2021.1 or newer (older versions might not work).

Your project must contain the following libraries somewhere:
- UPM: com.unity.modules.uielements (>= 1.0.0-preview.17; Official UPM Package; Name: UI Toolkit)
- Serilog.dll (>= 2.0.0; eg. from NuGet)
- UniRx.dll (>= 7.1.0; eg. from the asset store or from openupm: com.neuecc.unirx)
- SystemsRx.dll (>= 5.1.0; eg. from NuGet)
- SystemsRx.Infrastructure.dll (>= 5.1.0; eg. from NuGet)

## Installation
You have the following options:
1. The package is available on the [openupm registry](https://openupm.com). You can install it via [openupm-cli](https://github.com/openupm/openupm-cli).
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

The following snippet ilustrates how a mvvm binding can be created.
Your `IUxBinder` ties the elements from the binding to their models properties they should be syncronized with.
This has some boilerplate code but the provided extensions most cases very simplistic and it's very flexible.

With the `IUxBindingService` you can later make use of your `SettingsComponent` and the view binding that you have empowered it with.

```cs
using EcsRx.Groups;
using EcsRx.Groups.Observable;
using EcsRx.Plugins.GroupBinding.Attributes;
using EcsRx.Plugins.UnityUx;
using Serilog;
using SystemsRx.Infrastructure.Dependencies;
using SystemsRx.Infrastructure.Extensions;
using UniRx;
using UnityEngine.UIElements;

sealed class SettingsComponent : IUxComponent {
    public IReactiveProperty<string> PlayerName { get; set; }
    public IReactiveProperty<int> AutoSaveAfterTurns { get; set; }
    public IReactiveProperty<bool> AllowLogUpload { get; set; }
    public IReactiveProperty<float> MasterVolume { get; set; }
    public IReactiveProperty<float> MusicVolume { get; set; }
    public IReactiveProperty<float> SfxVolume { get; set; }
    public ISubject<Unit> Save { get; } = new Subject<Unit>();
}

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
        
        VisualElement interactableRoot; // container add dynamic ui onto
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

Apart from the simplest method in the `IUxBindingService` shown above it also supports more complicated scenarios like an `IUxComponent` embed inside of an IObservable or even a changing list by using `IReadOnlyReactiveCollection<IUxComponent>`.
The whole idea behind this is to also use the `IUxBindingService` inside of your IUxBinder if necessary to be able to bind nested `IUxComponent` structures.
