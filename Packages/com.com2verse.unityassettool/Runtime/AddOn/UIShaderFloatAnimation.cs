using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (MaskableGraphic))]
[ExecuteAlways]
public class UIShaderFloatAnimation : MonoBehaviour
{
    [Range(-1.0f,1.0f)]
    public float maskRange = 0.0f;
    public string propertieName = "_Range";

    public MaskableGraphic maskableGraphic;

    private void Awake()
    {
        this.maskRange = -1.0f;
    }

    private void Start()
    {
        this.maskableGraphic = this.GetComponent<MaskableGraphic>();
    }

    private void Update()
    {
        this.maskableGraphic.material.SetFloat(this.propertieName, this.maskRange);
    }
}
