using UnityEditor;

namespace Ximmerse.XR.Tag
{
    [CustomEditor(typeof(TagProfileLoading))]
    public class TagProfileLoadingEditor : Editor
    {
        private SerializedObject tagProfileLoading;
        private SerializedProperty Beacon;
        private SerializedProperty LiBeacon;
        private SerializedProperty TopoTag;
        private SerializedProperty SingleCard;
        private SerializedProperty SingleSize;
        private SerializedProperty Gun;
        private SerializedProperty TopoTagSize;
        private SerializedProperty GunType;
        private SerializedProperty LiBeaconType;

        void OnEnable()
        {
            tagProfileLoading = new SerializedObject(target);

            Beacon = tagProfileLoading.FindProperty("Beacon");
            LiBeacon = tagProfileLoading.FindProperty("LiBeacon");
            TopoTag = tagProfileLoading.FindProperty("TopoTag");
            SingleCard = tagProfileLoading.FindProperty("SingleCard");
            SingleSize = tagProfileLoading.FindProperty("singleSize");
            Gun = tagProfileLoading.FindProperty("Gun");
            TopoTagSize = tagProfileLoading.FindProperty("topoTagSize");
            GunType = tagProfileLoading.FindProperty("guntype");
            LiBeaconType = tagProfileLoading.FindProperty("liBeaconType");
        }

        public override void OnInspectorGUI()
        {
            tagProfileLoading.Update();
            SerializedProperty property = tagProfileLoading.GetIterator();
            while (property.NextVisible(true))
            {
                using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
                {
                    EditorGUILayout.PropertyField(property, true);
                    break;
                }
            }
            EditorGUILayout.PropertyField(Beacon);
            EditorGUILayout.PropertyField(LiBeacon);
            if (LiBeacon.boolValue)
            {
                EditorGUILayout.PropertyField(LiBeaconType);
            }
            EditorGUILayout.PropertyField(TopoTag);
            if (TopoTag.boolValue)
            {
                EditorGUILayout.PropertyField(TopoTagSize);
            }
            EditorGUILayout.PropertyField(SingleCard);
            if (SingleCard.boolValue)
            {
                EditorGUILayout.PropertyField(SingleSize);
            }
            EditorGUILayout.PropertyField(Gun);
            if (Gun.boolValue)
            {
                EditorGUILayout.PropertyField(GunType);
            }
            if (LiBeacon.boolValue && Gun.boolValue)
            {
                LiBeacon.boolValue = false;
                Gun.boolValue = false;
                UnityEditor.EditorUtility.DisplayDialog("Error", "Beacon type conflict", "х╥хо");
            }

            tagProfileLoading.ApplyModifiedProperties();
        }
    }
}


