using UnityEngine;

[ExecuteInEditMode]
public class ScannerController : MonoBehaviour
{
    public ScannerRenderFeature ScannerFeature;
    public Transform ScannerOrigin;
    [Range(0f, 10f)] public float ScanDistance = 2;

    private bool IsScanning;
    [SerializeField] private Material m_CachedMaterial;

    void Start()
    {
        m_CachedMaterial = ScannerFeature.GetPass().material;
    }

    void OnEnable()
    {
        // Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }

    void Update()
    {
        m_CachedMaterial.SetFloat("_ScanDistance", ScanDistance);
        m_CachedMaterial.SetVector("_WorldSpaceScannerPos", ScannerOrigin.position);

        if (IsScanning)
        {
            ScanDistance += Time.deltaTime * 10;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            IsScanning = true;
            ScanDistance = 0;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                IsScanning = true;
                ScanDistance = 0;
                ScannerOrigin.position = hit.point;
            }
        }
    }

}