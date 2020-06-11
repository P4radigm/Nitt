using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

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
    [SerializeField] private float comboCooldownTimer;
    [SerializeField] private float comboTextColoredTimer;
    [SerializeField] private AnimationCurve comboTextColoredCurve;
    [SerializeField] private float hitCooldown = 0.3f;
    [SerializeField] private AnimationCurve onHitPlayerGFXCurve;
    [SerializeField] private AnimationCurve onHitBackgroundGFXCurve;

    [Header("Player Stats")]
    public int hitPoints;
    public int maxHitPoints;
    public float teleportJuice;
    public float maxTeleportJuice;
    public float baseTpjuiceCost;
    public float tpDamageOutput;
    public float contactDamageOutput;
    public float teleportJuiceRegenMultiplier;
    public float teleportJuiceDrainMultiplier;
    public float teleportRange;
    public int currency;
    public int combo;

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
    [SerializeField] private ParticleSystem[] regenParticles;
    [SerializeField] private ParticleSystem[] drainParticles;
    [SerializeField] private ParticleSystem[] tpLossParticles;
    [SerializeField] private ParticleSystem TpParticles;
    [SerializeField] private TpUIColorManager tpAsset;
    [SerializeField] private AfterTpUIBar afterTpBar;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private ScoreManager scoreManager;

    private SpriteRenderer[] spR;
    private float fixedDeltaTime;
    private bool notTeleported = false;
    private bool beginPhaseMouse = true;
    private bool particlesAllowed = false;
    private bool regenAllowed = false;
    private bool firstDrain = true;
    private bool firstRegen = true;
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
    private float moveDirectionDistance = 0;

    [HideInInspector] public Vector2 lastGroundPos = Vector2.zero;
    [HideInInspector] public bool justTP = false;
    [HideInInspector] public bool playerNotHittable = false;

    private Coroutine comboCooldownRoutine;
    private Coroutine updateComboRoutine;

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

        comboText.text = "0";

        combo = 0;
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
            if (teleportJuice < maxTeleportJuice)
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

        if (teleportJuice < baseTpjuiceCost)
        {
            tpAsset.rainbowOn = true;
        }
        else
        {
            tpAsset.rainbowOn = false;
            tpAsset.GetComponent<SpriteRenderer>().color = Color.white;
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

        //TPJ cap
        if (teleportJuice > maxTeleportJuice)
        {
            teleportJuice = maxTeleportJuice;
        }

        //update UI
        healthSlider.value = hitPoints;
        teleportSlider.value = teleportJuice;
        currencyText.text = currency.ToString();
        playerCircleMask.transform.localScale = new Vector3((teleportRange * 2) / 0.9f, (teleportRange * 2) / 0.9f, (teleportRange * 2) / 0.9f);
        scoreManager.score = currency;


        //Death check
        if (hitPoints <= 0)
        {
            gm.DeathFade(this);
        }

        //max out TC when room is completed
        if (gm.activeRoom != null)
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

            for (int i = 0; i < tpLossParticles.Length; i++)
            {
                tpLossParticles[i].Play();
            }
        }

        beginPhaseMouse = true;
    }

    private void TPJuiceDrain()
    {
        firstRegen = true;

        afterTpBar.gameObject.SetActive(true);

        teleportJuice -= Time.deltaTime * teleportJuiceDrainMultiplier;

        if (firstDrain)
        {
            for (int i = 0; i < drainParticles.Length; i++)
            {
                drainParticles[i].gameObject.SetActive(true);
                drainParticles[i].Play();
            }
        }

        if (firstDrain)
        {
            for (int i = 0; i < regenParticles.Length; i++)
            {
                regenParticles[i].gameObject.SetActive(false);
                regenParticles[i].Stop();
            }
        }

        firstDrain = false;
        particlesAllowed = false;
        regenAllowed = false;
    }

    private void TPJuiceRegen()
    {
        firstDrain = true;

        afterTpBar.gameObject.SetActive(false);

        if (regenAllowed)
        {
            teleportJuice += Time.deltaTime * teleportJuiceRegenMultiplier;
        }

        if (particlesAllowed)
        {
            for (int i = 0; i < regenParticles.Length; i++)
            {
                regenParticles[i].gameObject.SetActive(true);
                regenParticles[i].Play();
            }
            particlesAllowed = false;
        }

        if (firstRegen)
        {
            for (int i = 0; i < drainParticles.Length; i++)
            {
                drainParticles[i].Stop();
            }
        }

        firstRegen = false;
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
        if (PostProcessingEffectsOffRoutine != null) { StopCoroutine(PostProcessingEffectsOffRoutine); }
        PostProcessingEffectsOffRoutine = StartCoroutine(IEPostProcessingEffectsOff());
    }

    private IEnumerator IEPostProcessingEffectsOff()
    {
        Vector2 screenPos = new Vector2(gm.mainCam.WorldToScreenPoint(teleportTargetGraphic.transform.position).x / gm.mainCam.scaledPixelWidth, gm.mainCam.WorldToScreenPoint(teleportTargetGraphic.transform.position).y / gm.mainCam.scaledPixelHeight);
        Vector2 offsetScreenPos = new Vector2(0.5f, 0.5f) + new Vector2(((screenPos.x - 0.5f) / 2) * ldOffset, ((screenPos.y - 0.5f) / 2) * ldOffset);
        ld.center.value = offsetScreenPos;

        float lerpTime = 0;

        while (lerpTime < 1)
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
        gm.healthLossParticles.Play();
        StartCoroutine(OnDamage());
    }

    public void GotUpgrade(UpgradeType upgradeType)
    {
        if (upgradeType == UpgradeType.MaxHpUpgrade)
        {
            maxHitPoints += gm.maxHPCellAmntInc;
            hitPoints += gm.maxHPCellAmntInc;

            healthSlider.maxValue = maxHitPoints;
        }
        else if (upgradeType == UpgradeType.MaxTpjUpgrade)
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

    public void CheckCombo(float cellRegenAmount)
    {
        int c = combo;
        float cc = combo;

        if(cellRegenAmount != 0)
        {
            teleportJuice += baseTpjuiceCost * (1/3) * c + (cellRegenAmount/(2/3));
            currency += Mathf.RoundToInt(cc/2-0.1f);
        }
    }

    private void UpdateComboText(int c, bool color)
    {
        if (updateComboRoutine == null)
        {
            updateComboRoutine = StartCoroutine(UpdateComboTextIE(c, color));
        }
        else
        {
            StopCoroutine(updateComboRoutine);
            updateComboRoutine = StartCoroutine(UpdateComboTextIE(c, color));
        }
    }

    private IEnumerator UpdateComboTextIE(int c, bool color)
    {
        comboText.text = combo.ToString();
        
        Color newCol = Color.HSVToRGB(Random.Range(0f, 1f), 1, 1);

        if (!color)
        {
            newCol = Color.black;
        }

        Color oldCol = Color.black;

        float lerpTime = 0;

        while (lerpTime < 1)
        {
            lerpTime += Time.deltaTime / comboTextColoredTimer;
            float EvaluatedLerpTime = comboTextColoredCurve.Evaluate(lerpTime);

            //SpriteRenderer sP = GetComponent<SpriteRenderer>();
            //sP.color = new Color(sP.color.r, sP.color.g, sP.color.b, EvaluatedLerpTime);

            float newR = Mathf.Lerp(oldCol.r, newCol.r, EvaluatedLerpTime);
            float newG = Mathf.Lerp(oldCol.g, newCol.g, EvaluatedLerpTime);
            float newB = Mathf.Lerp(oldCol.b, newCol.b, EvaluatedLerpTime);
            comboText.color = new Color(newR, newG, newB);

            yield return null;
        }
    }

    private IEnumerator JustTPCooldown()
    {
        yield return new WaitForSecondsRealtime(justTPTimer / 2);
        //check for combo
        if (gm.hitEnemy)
        {
            combo++;
            UpdateComboText(combo, true);
            gm.hitEnemy = false;
        }
        else
        {
            combo = 0;
            UpdateComboText(combo, false);
        }
        yield return new WaitForSecondsRealtime(justTPTimer/2);
        justTP = false;

        if(comboCooldownRoutine == null)
        {
            comboCooldownRoutine = StartCoroutine(ComboCooldownIE());
        }
        else
        {
            StopCoroutine(comboCooldownRoutine);
            comboCooldownRoutine = StartCoroutine(ComboCooldownIE());
        }
    }

    private IEnumerator ComboCooldownIE()
    {
        yield return new WaitForSeconds(comboCooldownTimer);
        combo = 0;
        UpdateComboText(combo, false);
    }

    private IEnumerator RegenCooldown()
    {
        yield return new WaitForSecondsRealtime(TPRegenCooldown - 1.15f);
        particlesAllowed = true;
        //Debug.Log("Ayy1");
        yield return new WaitForSecondsRealtime(1.15f);
        //Debug.Log("Ayy2");
        regenAllowed = true;
    }

    public IEnumerator OnDamage()
    {
        float lerpTime = 0;

        Physics2D.IgnoreLayerCollision(13, 11, true);
        Physics2D.IgnoreLayerCollision(13, 12, true);
        playerNotHittable = true;
        Physics2D.IgnoreLayerCollision(13, 12, false);
        Color oldColor = gm.mainCam.backgroundColor;

        //Debug.Log("player hit");

        while (lerpTime < 1)
        {
            lerpTime += Time.deltaTime / hitCooldown;
            float playerColorEvaluatedLerpTime = onHitPlayerGFXCurve.Evaluate(lerpTime);
            float backgroundColorEvaluatedLerpTime = onHitBackgroundGFXCurve.Evaluate(lerpTime);

            SpriteRenderer sP = GetComponent<SpriteRenderer>();
            sP.color = new Color(sP.color.r, sP.color.g, sP.color.b, playerColorEvaluatedLerpTime);

            float newR = Mathf.Lerp(oldColor.r, 0.8f, backgroundColorEvaluatedLerpTime);
            float newG = Mathf.Lerp(oldColor.g, 0.8f, backgroundColorEvaluatedLerpTime);
            float newB = Mathf.Lerp(oldColor.b, 0.8f, backgroundColorEvaluatedLerpTime);
            gm.mainCam.backgroundColor = new Color(newR, newG, newB);

            yield return null;
        }

        Physics2D.IgnoreLayerCollision(13, 11, false);
        playerNotHittable = false;

        //Debug.Log("player done hitting");
        yield return null;
    }
}
