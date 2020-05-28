using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    MaxHpUpgrade,
    MaxTpjUpgrade,
    TpjRegenUpgrade,
    TpRangeUpgrade,
    TpTimeSlowUpgrade,
    TpDamageUpgrade,
    ContactDamageUpgrade,
}

public class UpgradeSpawner : MonoBehaviour
{
    public UpgradeType setUpgradeType;
    [SerializeField] private bool randomUpgradeType;
    [SerializeField] private GameObject[] upgradePrefabs;
    [SerializeField] private ParticleSystem upgradeParticlesPrefab;

    private RoomManager roomPlacedIn;
    private GameObject spawnedObject;
    private GameManager gm;
    private ParticleSystem upgradeParticles;

    // Start is called before the first frame update
    void Start()
    { 
        gm = GameManager.instance;

        if (randomUpgradeType) { setUpgradeType = (UpgradeType)Random.Range(0, 7); }

        roomPlacedIn = transform.parent.GetComponent<RoomManager>();
        if(roomPlacedIn == null) { Debug.LogError("Upgrade Spawner not placed in a room"); }

        spawnedObject = Instantiate(upgradePrefabs[(int)setUpgradeType], transform.position, Quaternion.identity, transform);

        upgradeParticles = Instantiate(upgradeParticlesPrefab, transform.position, Quaternion.identity, transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.activeRoom == roomPlacedIn.gameObject && upgradeParticles != null)
        {
            upgradeParticles.gameObject.SetActive(true);

            if (!upgradeParticles.isPlaying)
            {
                upgradeParticles.Play();
            }
        }
        else if (upgradeParticles != null)
        {
            if (!upgradeParticles.isStopped)
            {
                upgradeParticles.Stop();
            }

            upgradeParticles.gameObject.SetActive(false);
        }

        if(spawnedObject == null && upgradeParticles != null)
        {
            if (!upgradeParticles.isStopped)
            {
                upgradeParticles.Stop();
                Destroy(upgradeParticles.gameObject, 5f);
            }
        }
    }
}
