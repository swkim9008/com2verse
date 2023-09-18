using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("UI/Effects/UISliderGradient")]
[ExecuteInEditMode]
[RequireComponent(typeof (Slider))]
public class UISliderGradient : MonoBehaviour
{
    private Slider slider;
    private MaskableGraphic fillRectImage;
    [SerializeField] private Gradient gradient = new Gradient();

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (this.slider == null || this.fillRectImage == null)
        {
            this.slider = this.GetComponent<Slider>();
            this.fillRectImage = this.slider.fillRect.GetComponent<MaskableGraphic>();
        }

        this.slider.onValueChanged.AddListener(value => ColorChange(value));
    }


    void ColorChange(float _value)
    {
        this.fillRectImage.color = this.gradient.Evaluate(_value);
    }

    private void Reset()
    {
        Init();
    }
}




#if UNITY_EDITOR
[CustomEditor(typeof(UISliderGradient))]
[CanEditMultipleObjects]
public class UISliderGradientEditor : Editor
{
    SerializedProperty gradient;

    void OnEnable()
    {
        gradient = serializedObject.FindProperty("gradient");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(gradient);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Reset")) 
        {
            ((UISliderGradient)target).Init(); ;
        };

        //base.OnInspectorGUI();
    }
}
#endif

