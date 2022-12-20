using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class ShakeVFXBounding : MonoBehaviour
{
    public Transform boundingObj;
    private VisualEffect vfx;

    void Start()
    {
        vfx = GetComponent<VisualEffect>();
    }

    void Update()
    {
        if (boundingObj == null)
            return;

        if (vfx.HasVector3("Position"))
        {
            // Debug.Log(vfx.visualEffectAsset.name);
            vfx.SetVector3("Position", boundingObj.transform.position - this.transform.position);
        }

    }

}