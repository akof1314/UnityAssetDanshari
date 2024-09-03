using System;
using System.Collections.Generic;
using UnityEditor;

namespace AssetDanshari
{
    public class AssetDanshariHandler
    {
        public static Func<AssetDanshariSetting> onCreateSetting;

        public static Action<GenericMenu> onDependenciesContextDraw;

        internal static Action<string> onDependenciesFindItem;

        public static Action<string, List<AssetTreeModel.AssetInfo>, AssetTreeModel> onDependenciesLoadDataMore;
    }
}