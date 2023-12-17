using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }
    public event EventHandler<int> OnScoreChanged;
    public event EventHandler OnIsGameRunningChanged;
    [SerializeField] private ParticleSystem mergeParticles;
    [SerializeField] private OrnamentListSO ornamentList;
    [SerializeField] private Transform leftBoundary;
    [SerializeField] private Transform rightBoundary;
    [SerializeField] private Transform bottomBoundary;
    [SerializeField] private Transform topBoundary;
    [SerializeField] private GameObject deathLine;

    private bool hasMerged = false;
    public bool CanPlace = true;
    public bool IsGamePaused = false;
    private int totalScore = 0;
    private GameObject nextOrnamentToSpawn;
    private GameObject currentOrnament;
    public bool IsGameRunning { get; private set; }
    private Vector3 lastPos = Vector3.zero;
    private void Awake() {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    private void Instance_OnCanUseInputChanged(object sender, bool canUseInput) {
        if (currentOrnament == null)
            return;
        if (canUseInput) {
            currentOrnament.SetActive(true);
        } else {
            currentOrnament.SetActive(false);
        }
    }

    private void Start() {
        InputManager.Instance.OnCanUseInputChanged += Instance_OnCanUseInputChanged;
        SetupBoundaries();
        PrepareNextOrnament();
        IsGameRunning = true;
    }

    private void Update() {
        if (IsGameRunning && !IsGamePaused) {
            MoveOrnament();
        }
    }

    public void ConnectOrnaments(Ornament Ornament1, Ornament Ornament2) {
        if (!hasMerged && IsGameRunning) {
            Destroy(Ornament1.gameObject);
            Destroy(Ornament2.gameObject);
            GameObject selectedOrnament = null;
            if (Ornament1.ornamentSize + 1 < ornamentList.OrnamentList.Count) {
                selectedOrnament = ornamentList.OrnamentList[Ornament1.ornamentSize + 1].prefab;
            }
            if (selectedOrnament != null) {
                Instantiate(selectedOrnament, Ornament1.transform.position, Quaternion.identity);
            }
            PlayParticles(Ornament1.transform);
            hasMerged = true;
            AudioManager.Instance.PlayConnectOrnamentSound();
            totalScore += ornamentList.OrnamentList[Ornament1.ornamentSize].score;
            OnScoreChanged?.Invoke(this, totalScore);
        }
        StartCoroutine(ResetHasMergedFlag());
    }

    private void PlayParticles(Transform ObjectTransform) {
        Vector3 particlePos = ObjectTransform.position;
        particlePos.z -= 1f;
        mergeParticles.transform.position = particlePos;
        mergeParticles.transform.localScale = ObjectTransform.transform.GetChild(0).transform.localScale * 3.5f;
        mergeParticles.Play();
    }

    private IEnumerator ResetHasMergedFlag() {
        yield return new WaitForFixedUpdate();
        hasMerged = false;
    }


    private void PrepareNextOrnament() {
        nextOrnamentToSpawn = ornamentList.OrnamentList[UnityEngine.Random.Range(0, 4)].prefab;
        if (nextOrnamentToSpawn != null) {
            Vector3 pos;
            if (lastPos == Vector3.zero) {
                float aboveDeathLine = 0.478f;
                pos = deathLine.transform.position + new Vector3(0f, aboveDeathLine, 0f);
            } else {
                pos = lastPos;
            }

            currentOrnament = Instantiate(nextOrnamentToSpawn, pos, Quaternion.identity);

            currentOrnament.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            currentOrnament.GetComponent<Ornament>().enabled = false;
            currentOrnament.GetComponentInChildren<Collider2D>().enabled = false;
        }
    }

    private void MoveOrnament() {
        if (currentOrnament != null && CanPlace && Mathf.Abs(InputManager.Instance.TouchXPos) < 1000000) {
            if (InputManager.Instance.TouchStarted || InputManager.Instance.TouchReleased) {
                Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector2(InputManager.Instance.TouchXPos, Screen.safeArea.yMax));
                //pos.y -= 1.4f;
                pos.z = 0;
                float aboveDeathLine = 0.478f;
                pos.y = deathLine.transform.position.y + aboveDeathLine;

                lastPos = pos;
                currentOrnament.transform.position = pos;
            }

            if (InputManager.Instance.TouchReleased) {
                AudioManager.Instance.PlayDropOrnamentSound();

                currentOrnament.GetComponent<Ornament>().enabled = true;
                currentOrnament.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                currentOrnament.GetComponentInChildren<Collider2D>().enabled = true;

                InputManager.Instance.TouchReleased = false;

                StartCoroutine(ResetCanPlace());
            }
        }
    }

    private IEnumerator ResetCanPlace() {
        CanPlace = false;
        yield return new WaitForSeconds(0.45f);
        CanPlace = true;
        PrepareNextOrnament();
    }
    private void SetupBoundaries() {
        leftBoundary.position = CalculateBoundaryPosition(Screen.safeArea.xMin, 0, -leftBoundary.GetComponent<BoxCollider2D>().size.x * 0.5f);
        rightBoundary.position = CalculateBoundaryPosition(Screen.safeArea.xMax, 0, rightBoundary.GetComponent<BoxCollider2D>().size.x * 0.5f);
        bottomBoundary.position = CalculateBoundaryPosition(0, Screen.safeArea.yMin, -bottomBoundary.GetComponent<BoxCollider2D>().size.y * 0.32f);
        topBoundary.position = CalculateBoundaryPosition(0, Screen.safeArea.yMax - 0.1f, 0);
        SetDeathLinePosition(topBoundary.gameObject);
    }
    private Vector3 CalculateBoundaryPosition(float x, float y, float offset) {
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector2(x, y));
        pos.x += offset;
        pos.y += offset;
        pos.z = 0;
        return pos;
    }
    private void SetDeathLinePosition(GameObject boundary) {
        Collider2D collider = boundary.GetComponent<Collider2D>();

        if (collider != null && collider.enabled) {
            Bounds colliderBounds = collider.bounds;
            float bottomY = colliderBounds.min.y;

            Vector3 bottomWorldPosition = boundary.transform.TransformPoint(new Vector3(0f, bottomY, 0f));

            deathLine.transform.position = new Vector3(deathLine.transform.position.x, bottomWorldPosition.y, deathLine.transform.position.z);
        }
    }

    public void GameOver() {
        if (!IsGameRunning)
            return;
        FileManager.Instance.SetScore(totalScore);
        IsGameRunning = false;
        OnIsGameRunningChanged?.Invoke(this, EventArgs.Empty);
        AudioManager.Instance.StopGameplayMusic();
    }

    public void SetIsGamePaused(bool isGamePaused) {
        IsGamePaused = isGamePaused;
    }
}