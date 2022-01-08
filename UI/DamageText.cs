using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    private FpsPlayer localPlayer;
    private Vector3 hitPoint;
    private Vector3 targetFloatTop;

    private void Awake()
    {
        localPlayer = LocalPlayerContext.Instance.player;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (localPlayer == null) return;
        hitPoint = Vector3.Lerp(hitPoint, targetFloatTop, 0.5f * Time.deltaTime);

        textMesh.transform.position = Camera.main.WorldToScreenPoint(hitPoint);
        if (Vector3.Dot((hitPoint - Camera.main.transform.position), Camera.main.transform.forward) < 0)
            textMesh.gameObject.SetActive(false);
        else
        {
            textMesh.gameObject.SetActive(true);
        }
    }

    public void Initialize(int damage, Vector3 position , bool isHeadshot)
    {
        localPlayer = LocalPlayerContext.Instance.player;
        textMesh.text = damage.ToString();
        hitPoint = position;

        targetFloatTop = hitPoint + new Vector3(0, 1.5f, 0);
        if (isHeadshot)
            textMesh.color = Color.red;
        else
            textMesh.color = Color.white;
    }

    public void SetText(int damage)
    {
        textMesh.text = damage.ToString();
    }
}
