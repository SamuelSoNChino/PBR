using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the menu scene.
/// </summary>
public class MenuManager : MonoBehaviour
{
    public List<GameObject> optionSets;
    public List<Vector3> setPlacements;

    private Vector2 startTouchPosition, currentTouchPosition, endTouchPosition, beforeTouchPosition;
    private bool isSwiping = false;

    Vector3 buttonsPosition;

    public float animationDuration = 0.5f;
    private int currentSetIndex = 0;
    private int prevSetIndex;
    private int nextSetIndex;
    private bool moveOutMutex = false;
    private bool moveInMutex = false;

    void Start()
    {
        // Při startu zobrazíme pouze první sadu možností
        for (int i = 0; i < optionSets.Count; i++)
        {
            optionSets[i].SetActive(i == currentSetIndex);
            setPlacements.Add(optionSets[i].transform.position);
        }
        UpdateAdjacentSetIndices();
        UpdateSetPositions();
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // Simulace swipování myší pro editor a standalone
        if (Input.GetMouseButtonDown(0) && !moveInMutex && !moveOutMutex)
        {
            startTouchPosition = Input.mousePosition;
            beforeTouchPosition = startTouchPosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButton(0) && isSwiping && !moveInMutex && !moveOutMutex)
        {
            currentTouchPosition = Input.mousePosition;
            Vector2 swipeDelta = (Vector2)currentTouchPosition - beforeTouchPosition;

            // Posuneme aktuální set podle pohybu myši
            MoveSets(swipeDelta.x);
            beforeTouchPosition = currentTouchPosition; // Aktualizujeme startovní pozici pro další frame
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping && !moveInMutex && !moveOutMutex)
        {
            endTouchPosition = Input.mousePosition;
            isSwiping = false;
            HandleSwipe();
        }
#else
        // Dotykové ovládání pro mobilní zařízení
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
                beforeTouchPosition = startTouchPosition;
                isSwiping = true;
            }
            else if (touch.phase == TouchPhase.Moved && isSwiping)
            {
                currentTouchPosition = touch.position;
                Vector2 swipeDelta = currentTouchPosition - beforeTouchPosition;

                // Posuneme aktuální set podle pohybu prstu
                MoveSets(swipeDelta.x);
                beforeTouchPosition = currentTouchPosition; // Aktualizujeme startovní pozici pro další frame
            }
            else if (touch.phase == TouchPhase.Ended && isSwiping)
            {
                endTouchPosition = touch.position;
                isSwiping = false;
                HandleSwipe();
            }
        }
#endif
    }

    private void MoveSets(float deltaX)
    {
        optionSets[currentSetIndex].transform.position += new Vector3(deltaX, 0, 0);
        optionSets[prevSetIndex].transform.position += new Vector3(deltaX, 0, 0);
        optionSets[nextSetIndex].transform.position += new Vector3(deltaX, 0, 0);
    }

    private void HandleSwipe()
    {
        Vector2 swipeDelta = endTouchPosition - startTouchPosition;

        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            if (swipeDelta.x > 0)
            {
                // Swipe doprava
                swipeRight();
            }
            else
            {
                // Swipe doleva
                swipeLeft();
            }
        }
        else
        {
            // Reset pozice, pokud swipe nebyl horizontální
            StartCoroutine(ResetPosition(optionSets[currentSetIndex]));
        }
    }

    private void UpdateAdjacentSetIndices()
    {
        nextSetIndex = (currentSetIndex + 1) % optionSets.Count;
        prevSetIndex = (currentSetIndex - 1 + optionSets.Count) % optionSets.Count;
    }

    private void UpdateSetPositions()
    {
        optionSets[currentSetIndex].transform.position = setPlacements[currentSetIndex];
        optionSets[prevSetIndex].transform.position = setPlacements[prevSetIndex] + Vector3.left * Screen.width;
        optionSets[nextSetIndex].transform.position = setPlacements[nextSetIndex] + Vector3.right * Screen.width;
        optionSets[prevSetIndex].SetActive(true);
        optionSets[nextSetIndex].SetActive(true);
    }

    public void swipeRight()
    {
        if (moveInMutex || moveOutMutex)
        {
            return;
        }
        moveInMutex = true;
        moveOutMutex = true;

        int setToMove = currentSetIndex;
        currentSetIndex = (currentSetIndex - 1 + optionSets.Count) % optionSets.Count;
        UpdateAdjacentSetIndices();

        StartCoroutine(MoveOut(optionSets[setToMove], Vector3.right, setPlacements[setToMove]));
        StartCoroutine(MoveIn(optionSets[currentSetIndex], Vector3.right, setPlacements[currentSetIndex]));
    }

    public void swipeLeft()
    {
        if (moveInMutex || moveOutMutex)
        {
            return;
        }
        moveInMutex = true;
        moveOutMutex = true;

        int setToMove = currentSetIndex;
        currentSetIndex = (currentSetIndex + 1) % optionSets.Count;
        UpdateAdjacentSetIndices();

        StartCoroutine(MoveOut(optionSets[setToMove], Vector3.left, setPlacements[setToMove]));
        StartCoroutine(MoveIn(optionSets[currentSetIndex], Vector3.left, setPlacements[currentSetIndex]));

    }

    private IEnumerator MoveOut(GameObject obj, Vector3 direction, Vector3 objPlacement)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = objPlacement + direction * Screen.width; // pohyb mimo obrazovku
        float distance = Vector3.Distance(startPos, endPos);
        float adjustedDuration = animationDuration * (distance / Screen.width); // úprava délky animace na základě vzdálenosti
        float elapsedTime = 0;

        while (elapsedTime < adjustedDuration)
        {
            obj.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / adjustedDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPos;
        moveOutMutex = false;
    }

    private IEnumerator MoveIn(GameObject obj, Vector3 direction, Vector3 objPlacement)
    {
        obj.SetActive(true); // Aktivace objektu před animací
        Vector3 startPos = obj.transform.position; // pozice mimo obrazovku
        Vector3 endPos = objPlacement; // konečná pozice
        float distance = Vector3.Distance(startPos, endPos);
        float adjustedDuration = animationDuration * (distance / Screen.width); // úprava délky animace na základě vzdálenosti
        float elapsedTime = 0;

        while (elapsedTime < adjustedDuration)
        {
            obj.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / adjustedDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPos;
        moveInMutex = false;

        UpdateSetPositions();
    }

    private IEnumerator ResetPosition(GameObject obj)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = setPlacements[currentSetIndex]; // Původní pozice uložená při startu

        float elapsedTime = 0;
        while (elapsedTime < animationDuration)
        {
            obj.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        UpdateSetPositions();
        obj.transform.position = endPos;
    }


    public void play()
    {
        switch (currentSetIndex)
        {
            case 0:
                StartSingleplayer();
                break;
            case 1:
                StartMultiplayer();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Loads the singleplayer puzzle scene.
    /// </summary>
    private void StartSingleplayer()
    {
        SceneManager.LoadScene("PuzzleSingleplayer");
    }

    /// <summary>
    /// Loads the multiplayer puzzle scene.
    /// </summary>
    private void StartMultiplayer()
    {
        SceneManager.LoadScene("Loadout");
    }

    /// <summary>
    /// Loads the options scene.
    /// </summary>
    private void GoToOptions()
    {
        SceneManager.LoadScene("Options");
    }
}
