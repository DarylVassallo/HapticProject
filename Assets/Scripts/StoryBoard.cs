using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class StoryBoard : MonoBehaviour
{
    [SerializeField] private Transform storyBoardParent;
    private Image[] _storyBoard;
    private int _currentStory;

    private void Awake()
    {
        _currentStory = 0;
        _storyBoard = new Image[storyBoardParent.childCount];
        for (int i = 0; i < storyBoardParent.childCount; i++)
        {
            _storyBoard[i] = storyBoardParent.GetChild(i).GetComponent<Image>();
            _storyBoard[i].color = new Color(1, 1, 1, 0);
        } 

        StartCoroutine(ShowStory(1f, _currentStory));
    }

    IEnumerator ShowStory(float _delay, int _storyNum)
    {
        Debug.Log("ShowStory: " + _storyNum);
        float elapsed = 0f;
        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            Debug.Log("_storyBoard[" + _storyNum + "]: " + _storyBoard[_storyNum]);
            _storyBoard[_storyNum].color = new Color(1, 1, 1, elapsed / _delay);
            yield return null;
        }

        _storyBoard[_storyNum].color = new Color(1, 1, 1, 1);
        _currentStory++;
        StartCoroutine(ShowStory(1f, _currentStory));
    }
}
