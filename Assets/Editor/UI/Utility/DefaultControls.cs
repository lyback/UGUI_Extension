using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    static class DefaultControls
    { 
        private static Vector2 s_ImageElementSize = new Vector2(100f, 100f);

        public static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        public static GameObject CreateLImage()
        {
            GameObject go = CreateUIElementRoot("LImage", s_ImageElementSize);
            go.AddComponent<LImage>();
            return go;
        }
		public static GameObject CreateLImageForTP()
        {
            GameObject go = CreateUIElementRoot("LImageForTP", s_ImageElementSize);
            go.AddComponent<LImageForTP>();
            return go;
        }
    }
}