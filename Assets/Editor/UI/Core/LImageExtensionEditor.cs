using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

namespace UnityEngine.UI
{
    public static class LImageExtensionEditor
    {
        [MenuItem("GameObject/UI/LImage", false, 2001)]
        static public void AddLImage(MenuCommand menuCommand)
        {
            GameObject go = CreateLImage();
            PlaceUIElementRoot(go, menuCommand);
        }
		[MenuItem("GameObject/UI/LImagePoly", false, 2001)]
        static public void AddLImagePoly(MenuCommand menuCommand)
        {
            GameObject go = CreateLImagePoly();
            PlaceUIElementRoot(go, menuCommand);
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
		[MenuItem("CONTEXT/Image/Image to LImagePoly")]
        static void Image2LImagePoly(MenuCommand menuCommand)
        {
            Image image = menuCommand.context as Image;
            GameObject go = image.gameObject;
            var color = image.color;
            var sprite = image.sprite;
            var raycastTarget = image.raycastTarget;

            var type = image.type;
            GameObject.DestroyImmediate(image);

            LImagePoly image2 = go.AddComponent<LImagePoly>();
            image2.color = color;
            image2.sprite = sprite;
            image2.raycastTarget = raycastTarget;
            image2.type = type;
        }
		[MenuItem("CONTEXT/LImage/LImage to LImagePoly")]
        static void LImage2LImagePoly(MenuCommand menuCommand)
        {
            LImage image = menuCommand.context as LImage;
            GameObject go = image.gameObject;
            var color = image.color;
            var sprite = image.sprite;
            var raycastTarget = image.raycastTarget;

            var type = image.type;
            GameObject.DestroyImmediate(image);

            LImagePoly image2 = go.AddComponent<LImagePoly>();
            image2.color = color;
            image2.sprite = sprite;
            image2.raycastTarget = raycastTarget;
            image2.type = type;
        }
#region DefaultControls.cs
        private static Vector2 s_ImageElementSize = new Vector2(100f, 100f);

        private static GameObject CreateUIElementRoot(string name, Vector2 size)
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
		public static GameObject CreateLImagePoly()
        {
            GameObject go = CreateUIElementRoot("LImagePoly", s_ImageElementSize);
            go.AddComponent<LImagePoly>();
            return go;
        }
#endregion

#region MenuOptions.cs
        private const string kUILayerName = "UI";

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            //if (parent != menuCommand.context) // not a context click, so center in sceneview
            //    SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }

        // Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        static public GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use just any canvas..
            canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }

        static public GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer(kUILayerName);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // if there is no event system add one...
            CreateEventSystem(false);
            return root;
        }

        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
            var esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }
#endregion
    }
}
