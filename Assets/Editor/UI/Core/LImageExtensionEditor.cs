using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;

namespace UnityEditor.UI
{
    public static class LImageExtensionEditor
    {
        [MenuItem("GameObject/UI/LImage", false, 2001)]
        static public void AddLImage(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateLImage();
            MenuOptions.PlaceUIElementRoot(go, menuCommand);
        }
		[MenuItem("GameObject/UI/LImageForTP", false, 2001)]
        static public void AddLImageForTP(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateLImageForTP();
            MenuOptions.PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("CONTEXT/Image/Image to LImage")]
        static void Image2LImage(MenuCommand menuCommand)
        {
            Image image = menuCommand.context as Image;
            GameObject go = image.gameObject;
            var color = image.color;
            var sprite = image.sprite;
            var raycastTarget = image.raycastTarget;

            var type = image.type;
            GameObject.DestroyImmediate(image);

            LImage image2 = go.AddComponent<LImage>();
            image2.color = color;
            image2.sprite = sprite;
            image2.raycastTarget = raycastTarget;
            image2.type = type;
        }

    }
}
