using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerBehaviour2 : MonoBehaviour
{
    [Header("Teleport tweaks")]
    [SerializeField] private float justTPTimer = 0.1f;
    [SerializeField] private float TPRegenCooldown = 2f;
    [SerializeField] private AnimationCurve tpAnimCurve;
    [SerializeField] private float tpAnimDuration;
    [SerializeField] private float minDistortionValue;
    [SerializeField] private float maxDistortionValue;
    [SerializeField] private float normalDistortionValue;
    [SerializeField] private float ldOffset;


    [Header("Game Feel")]
    [SerializeField] private float timeSlow = 0.5f;

    [Header("Player Stats")]
    public int hitPoints;
    public int maxHitPoints;
    public float teleportJuice;
    public float maxTeleportJuice; 
    public float baseTpjuiceCost;
    public float tpDamageOutput;
    public float contactDamageOutput;
    public int currency;
    public float teleportJuiceRegenMultiplier;
    public float teleportJuiceDrainMultiplier;
    public float teleportRange;

    [Header("Player Stats Tweaks")]
    [SerializeField] private int startMaxHitPoints;
    [SerializeField] private float startMaxTeleportJuice;
    [SerializeField] private float startBaseTpjuiceCost;
    [SerializeField] private float startTeleportJuiceRegenMultiplier;
    [SerializeField] private float startTeleportJuiceDrainMultiplier;
    [SerializeField] private float minTeleportJuiceForTP;
    [SerializeField] private float initialTpDamageOutput;
    [SerializeField] private float startTeleportRange;
    [SerializeField] private float inititalContactDamageOutput;
    [SerializeField] private int startCurrency;

    [Header("Needed")]
    [SerializeField] private GameObject teleportTargetGraphic = null;
    [SerializeField] private GameObject teleportTarget = null;
    [SerializeField] private SpriteRenderer brightnessFilter;
    [SerializeField] private SpriteMask playerCircleMask;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider teleportSlider;
    [SerializeField] private ParticleSystem regenParticles;
    [SerializeField] private ParticleSystem drainParticles;
    [SerializeField] private ParticleSystem TpParticles;

    private SpriteRenderer[] spR;
    private float fixedDeltaTime;
    private bool notTeleported = false;
    private bool beginPhaseMouse = true;
    private bool particlesAllowed = false;
    private bool regenAllowed = false;
    private VolumeProfile v;
    private LensDistortion ld;
    private Vignette vg;
    private ChromaticAberration cha;
    private Coroutine PostProcessingEffectsOffRoutine;
    //private float timeBtwTCRegenCounter;

    //Teleport Point Calculation
    private Vector2 teleportPoint = Vector2.zero;
    private Vector2 beginPointPos = Vector2.zero;
    private Vector2 endPointPos = Vector2.zero;
    private Vector2 moveDirection = Vector2.up;
    [HideInInspector] public Vector2 lastGroundPos = Vector2.zero;
    [HideInInspector] public bool justTP = false;
    private float moveDirectionDistance = 0;
    private GameManager gm;

    private Rigidbody2D playerRigidbody2D;

    private void Awake()
    {
        //Screen.SetResolution(2160, 1080, 0, 60);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        Input.simulateMouseWithTouches = true;
        playerRigidbody2D = GetComponent<Rigidbody2D>();

        maxTeleportJuice = startMaxTeleportJuice;
        maxHitPoints = startMaxHitPoints;
        hitPoints = maxHitPoints;
        teleportJuice = maxTeleportJuice;
        baseTpjuiceCost = startBaseTpjuiceCost;
        tpDamageOutput = initialTpDamageOutput;
        contactDamageOutput = inititalContactDamageOutput;
        currency = startCurrency;
        teleportJuiceRegenMultiplier = startTeleportJuiceRegenMultiplier;
        teleportJuiceDrainMultiplier = startTeleportJuiceDrainMultiplier;
        teleportRange = startTeleportRange;

        healthSlider.maxValue = maxHitPoints;
        teleportSlider.maxValue = maxTeleportJuice;

        playerCircleMask.transform.localScale = new Vector3(teleportRange * 2, teleportRange * 2, teleportRange * 2);
        brightnessFilter.enabled = false;

        spR = teleportTargetGraphic.GetComponentsInChildren<SpriteRenderer>();

        v = GameObject.FindGameObjectWithTag("TpProfile").GetComponent<Volume>()?.profile;
        v.TryGet(out ld);
        v.TryGet(out vg);
        v.TryGet(out cha);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        //Player Input
        if (Input.GetMouseButton(0) && teleportJuice > minTeleportJuiceForTP && teleportJuice > baseTpjuiceCost)
        {
            TimeSlow();
            CalcTargetPos();
            PostProcessingEffectsOn();
            if (!gm.activeRoom.GetComponent<RoomManager>().isCompleted) 
            { 
                TPJuiceDrain(); 
            }

            notTeleported = true;

            //graphics
            brightnessFilter.enabled = true;
            for (int i = 0; i < spR.Length; i++)
            {
                spR[i].enabled = true;
            }

            RayCastCheck1 ttRayCastCheck = teleportTarget.GetComponent<RayCastCheck1>();
            RayCastCheck1 ttgRayCastCheck = teleportTargetGraphic.GetComponent<RayCastCheck1>();

            teleportTarget.transform.position = teleportPoint;

            Vector2 tptpNeededAngle = teleportTarget.transform.position - teleportTargetGraphic.transform.position;
            float tptpNeededDistance = Vector2.Distance(teleportTargetGraphic.transform.position, teleportTarget.transform.position);
            RaycastHit2D tptpNeeded = Physics2D.Raycast(teleportTargetGraphic.transform.position, tptpNeededAngle.normalized, tptpNeededDistance, 1 << LayerMask.NameToLayer("Environment"));

            if (ttRayCastCheck.distIndex != ttgRayCastCheck.distIndex && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > 0 && ttRayCastCheck.distances[ttRayCastCheck.distIndex] <= ttgRayCastCheck.distances[ttgRayCastCheck.distIndex] && tptpNeeded.collider != null)
            {
                teleportTargetGraphic.transform.position = ttRayCastCheck.rayCastHits[ttRayCastCheck.distIndex].point + (ttRayCastCheck.rayCastHits[ttRayCastCheck.distIndex].normal) * (transform.localScale.x / 2);
            }
            else if (ttRayCastCheck.distIndex != ttgRayCastCheck.distIndex && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > 0 && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > ttgRayCastCheck.distances[ttgRayCastCheck.distIndex] && tptpNeeded.collider != null)
            {
                teleportTargetGraphic.transform.position = teleportTarget.transform.position;
            }
        }
        else
        {
            //TP
            if (notTeleported)
            {
                //Particles
                ParticleSystem tpP = Instantiate(TpParticles, transform.position, Quaternion.identity);
                tpP.Play();
                Destroy(tpP.gameObject, 0.6f);
                StartPostProcessingEffectsOff();

                TimeContinue();
                Teleport();
                StartCoroutine(RegenCooldown());

                notTeleported = false;
            }

            //TC regen
            if(teleportJuice < maxTeleportJuice)
            {
                TPJuiceRegen();
            }

            //if (timeBtwTCRegenCounter <= 0)
            //{
            //    teleportCells++;
            //    timeBtwTCRegenCounter = timeBtwTCRegen;
            //}
            //else if(teleportCells != maxTeleportCells)
            //{
            //    timeBtwTCRegenCounter -= Time.deltaTime;
            //}

            //GroundCheck
            RaycastHit2D groundRay = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, 1 << LayerMask.NameToLayer("Environment"));
            if (groundRay.collider != null)
            {
                //Debug.Log("new GroundPos = " + transform.position);
                lastGroundPos = transform.position;
            }

            //graphics
            brightnessFilter.enabled = false;
            for (int i = 0; i < spR.Length; i++)
            {
                spR[i].enabled = false;
            }

            //teleportTargetGraphic.SetActive(false);
        }

        //HP cap
        if (hitPoints > maxHitPoints)
        {
            hitPoints = maxHitPoints;
        }

        //TC cap
        //if (teleportCells > maxTeleportCells)
        //{
        //    teleportCells = maxTeleportCells;
        //}

        //update UI
        healthSlider.value = hitPoints;
        teleportSlider.value = teleportJuice;

        //Death check
        if (hitPoints == 0)
        {
            Death();
        }

        //max out TC when room is completed
        if(gm.activeRoom != null)
        {
            if (gm.activeRoom.GetComponent<RoomManager>().isCompleted)
            {
                teleportJuice = maxTeleportJuice;
                //timeBtwTCRegenCounter = timeBtwTCRegen;
            }
        }
    }

    private void TimeSlow()
    {
        Time.timeScale = timeSlow;
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
    }

    private void TimeContinue()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
    }

    private void CalcTargetPos()
    {
        if (beginPhaseMouse == true)
        {
            beginPointPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            beginPhaseMouse = false;
            teleportTargetGraphic.transform.position = transform.position;
        }

        endPointPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        moveDirection = endPointPos - beginPointPos;
        moveDirection.Normalize();
        //moveDirectionAngle = Mathf.Atan2(moveDirection.x, moveDirection.y) * Mathf.Rad2Deg;

        moveDirectionDistance = Mathf.Clamp(Vector2.Distance(endPointPos, beginPointPos), 0f, teleportRange);

        teleportPoint = new Vector2(transform.position.x, transform.position.y) + (moveDirection * moveDirectionDistance);
    }

    private void Teleport()
    {
        transform.position = teleportTargetGraphic.transform.position;
        playerRigidbody2D.velocity = Vector2.zero;
        //teleportTargetGraphic.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        justTP = true;
        StartCoroutine(JustTPCooldown());

        if (!gm.activeRoom.GetComponent<RoomManager>().isCompleted)
        {
            teleportJuice -= baseTpjuiceCost;
        }

        beginPhaseMouse = true;
    }

    private void TPJuiceDrain()
    {
        teleportJuice -= Time.deltaTime * teleportJuiceDrainMultiplier;
        if (!drainParticles.isPlaying)
        {
            drainParticles.gameObject.SetActive(true);
            drainParticles.Play();
        }

        if (!regenParticles.isStopped)
        {
            regenParticles.gameObject.SetActive(false);
            regenParticles.Stop();
            ParticleSystem _System;
            _System = regenParticles;
            ParticleSystem.Particle[] _Particles;
            _Particles = new ParticleSystem.Particle[_System.main.maxParticles];

            for (int i = 0; i < _Particles.Length; i++)
            {
                _Particles[i].velocity = Vector3.zero;
            }
        }

        particlesAllowed = false;
        regenAllowed = false;
    }

    private void TPJuiceRegen()
    {
        if (regenAllowed)
        {
            teleportJuice += Time.deltaTime * teleportJuiceRegenMultiplier;
        }

        if (!regenParticles.isPlaying && particlesAllowed == true)
        {
            regenParticles.gameObject.SetActive(true);
            //Debug.Log("Ayy1");
            regenParticles.Play();
        }

        if (!drainParticles.isStopped)
        {
            drainParticles.Stop();
            ParticleSystem _System;
            _System = drainParticles;
            ParticleSystem.Particle[] _Particles;
            _Particles = new ParticleSystem.Particle[_System.main.maxParticles];

            for (int i = 0; i < _Particles.Length; i++)
            {
                _Particles[i].velocity = Vector3.zero;
            }
        }
    }

    private void PostProcessingEffectsOn()
    {
        ld.active = true;
        vg.active = true;
        cha.active = true;

        ld.intensity.value = normalDistortionValue;
        ld.center.value = new Vector2(gm.mainCam.WorldToScreenPoint(teleportTargetGraphic.transform.position).x / gm.mainCam.scaledPixelWidth, gm.mainCam.WorldToScreenPoint(teleportTargetGraphic.transform.position).y / gm.mainCam.scaledPixelHeight);

        vg.center.value = new Vector2(gm.mainCam.WorldToScreenPoint(teleportTargetGraphic.transform.position).x / gm.mainCam.scaledPixelWidth, gm.mainCam.WorldToScreenPoint(teleportTargetGraphic.transform.position).y / gm.mainCam.scaledPixelHeight);

    }

    private void StartPostProcessingEffectsOff()
    {
        if(PostProcessingEffectsOffRoutine != null) { StopCoroutine(PostProcessingEffectsOffRoutine); }
        PostProcessingEffectsOffRoutine = StartCoroutine(IEPostProcessingEffectsOff());
    }

    private IEnumerator IEPostProcessingEffectsOff()
    {
        Vector2 screenPos = new Vector2(gm.mainCam.WorldToScreenPoint(teleportTargetGraphic.transform.position).x / gm.mainCam.scaledPixelWidth, gm.mainCam.WorldToScreenPoint(teleportTargetGraphic.transform.position).y / gm.mainCam.scaledPixelHeight);
        Vector2 offsetScreenPos = new Vector2(0.5f, 0.5f) + new Vector2(((screenPos.x - 0.5f) / 2) * ldOffset, ((screenPos.y - 0.5f) / 2) * ldOffset);
        ld.center.value = offsetScreenPos;

        float lerpTime = 0;

        while(lerpTime < 1)
        {
            lerpTime += Time.deltaTime / tpAnimDuration;
            float evaluatedLerpTime = tpAnimCurve.Evaluate(lerpTime);
            float newIntesityValue = Mathf.Lerp(minDistortionValue, maxDistortionValue, evaluatedLerpTime);

            ld.intensity.value = newIntesityValue;

            yield return null;
        }

        ld.active = false;
        vg.active = false;
        cha.active = false;

        yield return null;
    }

    public void EnvironmentDamage()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.position = lastGroundPos;
        hitPoints--;
    }

    private void Death()
    {
        SceneManager.LoadScene("Menu");
    }

    public void GotUpgrade(UpgradeType upgradeType)
    {
        if(upgradeType == UpgradeType.MaxHpUpgrade)
        {
            maxHitPoints += gm.maxHPCellAmntInc;
            hitPoints += gm.maxHPCellAmntInc;

            healthSlider.maxValue = maxHitPoints;
        }
        else if(upgradeType == UpgradeType.MaxTpjUpgrade)
        {
            maxTeleportJuice += gm.maxTpjAmntInc;

            teleportSlider.maxValue = maxTeleportJuice;
        }
        else if (upgradeType == UpgradeType.TpjRegenUpgrade)
        {
            teleportJuiceRegenMultiplier += gm.tpjRegenAmntInc;
        }
        else if (upgradeType == UpgradeType.TpRangeUpgrade)
        {
            teleportRange += gm.tpRangeAmntInc;
        }
        else if (upgradeType == UpgradeType.TpTimeSlowUpgrade)
        {
            timeSlow -= gm.tpTimeSlowAmntDec;
        }
        else if (upgradeType == UpgradeType.TpDamageUpgrade)
        {
            tpDamageOutput += gm.tpDamageAmntInc;
        }
        else if (upgradeType == UpgradeType.ContactDamageUpgrade)
        {
            contactDamageOutput += gm.contactDamageAmntInc;
        }
    }

    private IEnumerator JustTPCooldown()
    {
        yield return new WaitForSeconds(justTPTimer);
        justTP = false;
    }

    private IEnumerator RegenCooldown()
    {
        yield return new WaitForSeconds(TPRegenCooldown - 1.3f);
        particlesAllowed = true;
        //Debug.Log("Ayy1");
        yield return new WaitForSeconds(1.3f);
        //Debug.Log("Ayy2");
        regenAllowed = true;
    }
}
