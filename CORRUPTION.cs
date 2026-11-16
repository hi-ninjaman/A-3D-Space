using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CORRUPTION : MonoBehaviour
{
    public Material[] corruptionMaterial;
    private Renderer[] allParts;
    public Material newSkybox;

    public float timer = 0f;
    public bool timerEnabled = false;
    public bool isActivated;
    public TMP_Text timerText;

    public GameObject EXIT;
    public GameObject GATE;
    public GameObject TIMER;
    public Transform PORTAL;
    public float portalRadius;
    public LayerMask playerLayer;
    void Start()
    {
        //finds all of the parts for the corrupotion thing
        allParts = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
    }
    
    void Update()
    {
        // this detects if someone enters the area of the portal
        Collider[] sensed = Physics.OverlapSphere(PORTAL.transform.position, portalRadius, playerLayer);

        if (sensed.Length > 0 && !isActivated)
        {
            isActivated = true;
        }

        if(isActivated)
        {
            EXIT.SetActive(true);
            GATE.SetActive(true);
            timerEnabled = true;

            RenderSettings.skybox = newSkybox;
            DynamicGI.UpdateEnvironment();
            foreach (Renderer part in allParts)
            {
                if (part != null && !part.CompareTag("Player"))
                {
                    int randomIndex = Random.Range(0, corruptionMaterial.Length);
                    part.material = corruptionMaterial[randomIndex];
                    Debug.Log("Corrupted:" + part.gameObject.name);
                }
            }
        }
        if (timerEnabled)
        {
            TIMER.SetActive(true);
            timer -= Time.deltaTime;
            timer = Mathf.Max(timer, 0f);
            timerText.text = "[00:" + Mathf.CeilToInt(timer).ToString("00") + "]";
            if (timer <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
