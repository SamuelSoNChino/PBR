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

    private Vector2 startTouchPosition, currentTouchPosition, endTouchPosition, beforeTouchPosition;
    private bool isSwiping = false;

    public float animationDuration = 0.5f;
    private int currentSetIndex = 0;
    private bool moveOutMutex = false;
    private bool moveInMutex = false;

    void Start()
    {
        // Při startu zobrazíme pouze první sadu možností
        for (int i = 0; i < optionSets.Count; i++)
        {
            optionSets[i].SetActive(i == currentSetIndex);
        }
    }

    void Update()
    {
        #if UNITY_EDITOR || UNITY_STANDALONE
        // Simulace swipování myší pro editor a standalone
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            beforeTouchPosition = startTouchPosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButton(0) && isSwiping)
        {
            currentTouchPosition = Input.mousePosition;
            Vector2 swipeDelta = (Vector2)currentTouchPosition - beforeTouchPosition;

            // Posuneme aktuální set podle pohybu myši
            optionSets[currentSetIndex].transform.position += new Vector3(swipeDelta.x, 0, 0);
            beforeTouchPosition = currentTouchPosition; // Aktualizujeme startovní pozici pro další frame
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
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
                optionSets[currentSetIndex].transform.position += new Vector3(swipeDelta.x, 0, 0);
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

    public void swipeRight()
    {
        if (moveInMutex || moveOutMutex)
        {
            return;
        }
        int nextSetIndex = (currentSetIndex + 1) % optionSets.Count;
        moveInMutex = true;
        moveOutMutex = true;
        StartCoroutine(MoveOut(optionSets[currentSetIndex], Vector3.right));
        StartCoroutine(MoveIn(optionSets[nextSetIndex], Vector3.right));
        currentSetIndex = nextSetIndex;
    }


    public void swipeLeft()
    {
        if (moveInMutex || moveOutMutex)
        {
            return;
        }
        int nextSetIndex = (currentSetIndex - 1 + optionSets.Count) % optionSets.Count;
        moveInMutex = true;
        moveOutMutex = true;
        StartCoroutine(MoveOut(optionSets[currentSetIndex], Vector3.left));
        StartCoroutine(MoveIn(optionSets[nextSetIndex], Vector3.left));
        currentSetIndex = nextSetIndex;
    }


    private IEnumerator MoveOut(GameObject obj, Vector3 direction)
    {

        Vector3 buttonsPosition = new(642, 1089.02F, 0);
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = buttonsPosition + direction * Screen.width; // pohyb mimo obrazovku
        float elapsedTime = 0;

        while (elapsedTime < animationDuration)
        {
            obj.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / animationDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.SetActive(false); // Deaktivace objektu po animaci
        obj.transform.position = buttonsPosition;
        moveOutMutex = false;
    }

    private IEnumerator MoveIn(GameObject obj, Vector3 direction)
    {
        Vector3 buttonsPosition = new(642, 1089.02F, 0);
        obj.SetActive(true); // Aktivace objektu před animací
        Vector3 startPos = buttonsPosition - direction * Screen.width; // pozice mimo obrazovku
        Vector3 endPos = buttonsPosition; // konečná pozice
        obj.transform.position = startPos;
        float elapsedTime = 0;

        while (elapsedTime < animationDuration)
        {
            obj.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / animationDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPos;
        moveInMutex = false;
    }

    private IEnumerator ResetPosition(GameObject obj)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = new Vector3(0, 0, startPos.z); // Původní pozice ve středu

        float elapsedTime = 0;
        while (elapsedTime < animationDuration)
        {
            obj.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / animationDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPos;
    }

    /// <summary>
    /// Loads the singleplayer puzzle scene.
    /// </summary>
    public void StartSingleplayer()
    {
        SceneManager.LoadScene("PuzzleSingleplayer");
    }

    /// <summary>
    /// Loads the multiplayer puzzle scene.
    /// </summary>
    public void StartMultiplayer()
    {
        SceneManager.LoadScene("PuzzleMultiplayer");
    }

    /// <summary>
    /// Loads the options scene.
    /// </summary>
    public void GoToOptions()
    {
        SceneManager.LoadScene("Options");
    }
}
