# 用户指南

本指南旨在简要介绍 UnityAssetDanshari 及其功能概述。

## 简介

UnityAssetDanshari 是一个 Unity 资源清理重复以及引用被引用查找工具。内核使用 ripgrep 文本搜索工具，使得资源查找速度飞快。

## 首次使用

提前准备 ripgrep 的二进制文件[https://github.com/BurntSushi/ripgrep/releases](https://github.com/BurntSushi/ripgrep/releases)， 也可以直接使用示例工程自带的 Windows 版二进制文件。

![](./Images/MainWindows.png)

工具入口为菜单栏【美术工具/资源断舍离】，如果导入了示例工程，则打开会包含默认的配置项。点击右上角的`ripgrep`按钮，可以进行配置`ripgrep路径`。如果没有配置`ripgrep路径`则会使用默认的普通文件搜索方式，速度相当慢。

## 默认配置项

工具配置文件保存在`"UserSettings/AssetDanshariSetting.asset"`，为了能够团队默认能够有一样的配置内容，可以绑定创建配置事件。

```csharp
[InitializeOnLoadMethod]
private static void InitOnLoad()
{
    AssetDanshariHandler.onCreateSetting += OnCreateSetting;
    AssetDanshariHandler.onDependenciesLoadDataMore += OnDependenciesLoadDataMore;
    AssetDanshariHandler.onDependenciesContextDraw += OnDependenciesContextDraw;
}
```

用户自定义配置事件，可以指定rg到工程的统一外部工具路径。

```csharp
private static AssetDanshariSetting OnCreateSetting()
{
    var setting = ScriptableObject.CreateInstance<AssetDanshariSetting>();
    setting.ripgrepPath = "Assets/Samples/Asset Danshari/1.0.0/Simple Demo/rg/rg.exe";
    setting.assetReferenceInfos.Add(new AssetDanshariSetting.AssetReferenceInfo()
    {
        referenceFolder = "\"Assets/Samples/Asset Danshari/1.0.0/Simple Demo/Prefab\" || \"Assets/Samples/Asset Danshari/1.0.0/Simple Demo/Samples\"",
        assetFolder = "\"Assets/Samples/Asset Danshari/1.0.0/Simple Demo/PNG\"",
        assetCommonFolder = "\"Assets/Samples/Asset Danshari/1.0.0/Simple Demo/PNG/Common\""
    });
    return setting;
}
```

## 检查列表

列表按资源的使用环境进行分组。
- 将`Project`窗口里的文件或文件夹路径拖到目录框
- 多路径方式可以多选后再拖入，也可以按住`Ctrl`进行添加
- 【公共资源目录】是用来放公共资源的路径，比如 UI 图片资源存在被多个界面引用的时候，可以快捷操作移动资源到公共目录

## 引用查找

对【引用目录】下的每个资源进行检查是否引用到了【资源目录】下的资源，比如 UI 界面预制引用 UI 图片。

![](https://img-blog.csdnimg.cn/20181110160138165.png)

双击项，可以自动在【Project】窗口定位到资源。

## 被引用查找

对【资源目录】下的每个资源进行分析，看是否被【引用目录】下的资源进行引用，比如 UI 图片被哪些 UI 界面进行引用。

![](https://img-blog.csdnimg.cn/20181110160334244.png)

【删除选中资源】菜单项功能，是直接对资源进行删除，当发现没有被使用到时，可以这样快捷删除资源。

右上角【过滤为空】按钮，可以过滤显示没有被使用的资源，方便快速查看。

## 检查重复

对资源文件进行逐块 Hash128 检查重复，再对重复的资源进行操作。

![](https://img-blog.csdnimg.cn/20181110160230435.png)


【资源被引用查找】菜单项功能，是在【被引用查找】窗口里定位到此资源的使用情况，方便进行决定保留还是删除，注意，需要先打开了【被引用查找】窗口才可以定位到。

【仅使用此资源，其余删除】菜单项功能，将会删除其余重复的资源，并且将所有引用到这些删除资源的地方都改成引用保留的那一个。

当美术对同一资源进行切图两次，会导致文件 Hash128 值不一样，就无法被工具所检测到。出现这种情况的时候，肉眼发现到两个资源其实是一样的，可以在这个窗口右上角点击【手动添加】按钮。

![](https://img-blog.csdnimg.cn/20181110160308811.png)

手动进行添加资源路径，将资源文件拖动到文本框，再点击【确定】。就会自动定位到新增的数据，接着就可以按处理重复资源一样进行操作。

## 扩展被引用来源

![](./Images/CustomDependSource.png)

资源不止被另外的资源直接引用，还可能配置在表里、代码里进行动态使用，在这种情况下，可以绑定`onDependenciesLoadDataMore`事件，对传入的资源路径进行判别处理，参照示例工程代码。

