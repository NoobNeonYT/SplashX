using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashX_PerkSystem : MonoBehaviour
{
    [Header("Perk Settings")]
    public int maxPerk = 100;
    public int currentPerk = 0;
    public Image perkBarFill;
    public KeyCode usePerkKey = KeyCode.O;

    [Header("UI Selection (ลาก Image ของแต่ละสกิลมาใส่)")]
    public RectTransform skill1UI;
    public RectTransform skill2UI;
    public RectTransform skill3UI;
    public Vector3 normalScale = new Vector3(1f, 1f, 1f);
    public Vector3 selectedScale = new Vector3(1.3f, 1.3f, 1f);

    // 🔥 1 = ดูดเลือด, 2 = ชะลอเวลา, 3 = บ้าคลั่ง
    private int selectedSkill = 1;

    [Header("Audio Settings")]
    public AudioSource audioSource; // ลาก AudioSource ของตัวละครมาใส่
    public AudioClip skill1SFX; // เสียงดูดเลือด
    public AudioClip skill2SFX; // เสียงชะลอเวลา (Za Warudo!)
    public AudioClip skill3SFX; // เสียงคำรามบ้าคลั่ง

    // ==========================================

    [Header("Skill 1: Life Steal")]
    public float stealRadius = 10f;
    public LayerMask enemyLayers;
    public GameObject bloodVFXPrefab;
    public GameObject overhealSparkVFX;

    [Header("Skill 2: ฮะ ฮ่า ช้าชะมัด (Time Slow)")]
    public float slowDuration = 10f;
    public float timeScaleAmount = 0.4f;

    [Header("Skill 3: Berserk Mode")]
    public float berserkDuration = 10f;
    public int berserkBonusDamage = 20;
    public float berserkDamageReduction = 0.5f;
    public GameObject berserkAuraVFX;
    public float shakeIntensity = 0.1f;
    public bool isBerserk = false;

    // ==========================================

    private SplashX_PlayerMovement playerMovement;
    private SplashX_PlayerStats playerStats;
    private Rigidbody2D rb;
    private bool isPerkActive = false;

    void Start()
    {
        playerMovement = GetComponent<SplashX_PlayerMovement>();
        playerStats = GetComponent<SplashX_PlayerStats>();
        rb = GetComponent<Rigidbody2D>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        UpdatePerkUI();
        UpdateSkillSelectionUI(1); // เริ่มต้นเลือกสกิล 1 (ดูดเลือด)
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) UpdateSkillSelectionUI(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) UpdateSkillSelectionUI(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) UpdateSkillSelectionUI(3);

        if (Input.GetKeyDown(usePerkKey) && currentPerk >= maxPerk && !isPerkActive)
        {
            ActivateSelectedSkill();
        }

        if (isBerserk && Mathf.Abs(rb.linearVelocity.x) > 0.1f && playerMovement != null)
        {
            // CameraShake.Instance.ShakeCamera(shakeIntensity, 0.1f); 
        }
    }

    private void UpdateSkillSelectionUI(int skillIndex)
    {
        selectedSkill = skillIndex;

        if (skill1UI != null) skill1UI.localScale = normalScale;
        if (skill2UI != null) skill2UI.localScale = normalScale;
        if (skill3UI != null) skill3UI.localScale = normalScale;

        if (skillIndex == 1 && skill1UI != null) skill1UI.localScale = selectedScale;
        else if (skillIndex == 2 && skill2UI != null) skill2UI.localScale = selectedScale;
        else if (skillIndex == 3 && skill3UI != null) skill3UI.localScale = selectedScale;
    }

    private void ActivateSelectedSkill()
    {
        currentPerk = 0;
        UpdatePerkUI();
        isPerkActive = true;

        Debug.Log("🔥 กดใช้สกิลที่: " + selectedSkill);

        if (selectedSkill == 1) Skill1_LifeSteal();
        else if (selectedSkill == 2) StartCoroutine(Skill2_TimeSlowRoutine());
        else if (selectedSkill == 3) StartCoroutine(Skill3_BerserkRoutine());
    }

    // 🧛 สกิล 1: ดูดเลือด
    private void Skill1_LifeSteal()
    {
        PlaySFX(skill1SFX);
        Debug.Log("🧛 [Life Steal] เริ่มดูดเลือด!");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, stealRadius, enemyLayers);
        int totalHealed = 0;

        foreach (Collider2D enemyHit in hits)
        {
            SplashX_Enemy enemy = enemyHit.GetComponent<SplashX_Enemy>();
            if (enemy != null)
            {
                int stealAmount = Random.Range(5, 11);
                enemy.TakeDamage(stealAmount);
                totalHealed += stealAmount;

                if (bloodVFXPrefab != null) Instantiate(bloodVFXPrefab, enemyHit.transform.position, Quaternion.identity);
            }
        }

        if (totalHealed > 0 && playerStats != null)
        {
            playerStats.currentHp += totalHealed;
            if (playerStats.currentHp > playerStats.maxHp)
            {
                if (overhealSparkVFX != null)
                {
                    GameObject spark = Instantiate(overhealSparkVFX, transform.position, Quaternion.identity);
                    spark.transform.SetParent(transform);
                }
            }
        }

        isPerkActive = false;
    }

    // 🕒 สกิล 2: ฮะ ฮ่า ช้าชะมัด
    // 🕒 สกิล 2: ฮะ ฮ่า ช้าชะมัด
    private IEnumerator Skill2_TimeSlowRoutine()
    {
        PlaySFX(skill2SFX);
        Debug.Log("🕒 [Time Slow] เริ่ม! Za Warudo!");

        // ทำให้เกมช้าลง และปรับฟิสิกส์ให้สมูท
        Time.timeScale = timeScaleAmount;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // คำนวณตัวคูณ (เช่น ช้าลง 0.4 ก็ต้องเร่งสปีดผู้เล่น 2.5 เท่า)
        float timeMultiplier = 1f / timeScaleAmount;

        // 🔥 เรียกใช้คำสั่งบัพทุกความเร็วและแรงโน้มถ่วงให้ผู้เล่น!
        if (playerMovement != null) playerMovement.ApplyTimeScaleMultiplier(timeMultiplier);

        // ปลดล็อกแอนิเมชันให้เล่นความเร็วปกติ
        if (playerMovement.boneAnim != null) playerMovement.boneAnim.updateMode = AnimatorUpdateMode.UnscaledTime;
        if (playerMovement.fbfAnim != null) playerMovement.fbfAnim.updateMode = AnimatorUpdateMode.UnscaledTime;

        // รอเวลาตามโลกความเป็นจริง
        yield return new WaitForSecondsRealtime(slowDuration);

        // 🔄 คืนค่าเวลาของเกม
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // 🔥 คืนค่าความเร็วผู้เล่นกลับเป็นปกติ (โดยการเอาเปอร์เซ็นต์มาคูณกลับ)
        if (playerMovement != null) playerMovement.ApplyTimeScaleMultiplier(timeScaleAmount);

        // คืนค่าแอนิเมชัน
        if (playerMovement.boneAnim != null) playerMovement.boneAnim.updateMode = AnimatorUpdateMode.Normal;
        if (playerMovement.fbfAnim != null) playerMovement.fbfAnim.updateMode = AnimatorUpdateMode.Normal;

        Debug.Log("🕒 [Time Slow] เวลาเดินต่อตามปกติแล้ว!");
        isPerkActive = false;
    }

    // 😡 สกิล 3: บ้าคลั่ง
    private IEnumerator Skill3_BerserkRoutine()
    {
        PlaySFX(skill3SFX);
        Debug.Log("😡 [Berserk] โหมดบ้าคลั่งทำงาน!");

        isBerserk = true;
        if (berserkAuraVFX != null) berserkAuraVFX.SetActive(true);

        playerMovement.hit1Damage += berserkBonusDamage;
        playerMovement.hit2Damage += berserkBonusDamage;
        playerMovement.hit3Damage += berserkBonusDamage;

        yield return new WaitForSeconds(berserkDuration);

        isBerserk = false;
        if (berserkAuraVFX != null) berserkAuraVFX.SetActive(false);
        playerMovement.hit1Damage -= berserkBonusDamage;
        playerMovement.hit2Damage -= berserkBonusDamage;
        playerMovement.hit3Damage -= berserkBonusDamage;

        Debug.Log("😡 [Berserk] หมดโหมดบ้าคลั่ง!");
        isPerkActive = false;
    }

    public void AddPerkPoints(int points)
    {
        if (currentPerk >= maxPerk) return;
        currentPerk += points;
        if (currentPerk > maxPerk) currentPerk = maxPerk;
        UpdatePerkUI();
    }

    private void UpdatePerkUI()
    {
        if (perkBarFill != null) perkBarFill.fillAmount = (float)currentPerk / maxPerk;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stealRadius);
    }
}