using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using Unity.Netcode;

public class StoryBoard : NetworkBehaviour
{
    private bool _playStoryBoard;

    [SerializeField] private AudioClip[] _storyBoardAudioClips;
    private AudioSource _audioSource;

    [SerializeField] private Transform pcStoryBoardParent;
    private Image[] _pcStoryBoard;
    [SerializeField] private Transform vrStoryBoardParent;
    private SpriteRenderer[] _vrStoryBoard;

    private int _currentStory;
    private int _maxStoryNum;

    private void Awake()
    {
        _playStoryBoard = false;
        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _currentStory = 0;
        _maxStoryNum = pcStoryBoardParent.childCount;

        _pcStoryBoard = new Image[pcStoryBoardParent.childCount];
        for (int i = 0; i < pcStoryBoardParent.childCount; i++)
        {
            _pcStoryBoard[i] = pcStoryBoardParent.GetChild(i).GetComponent<Image>();
            _pcStoryBoard[i].color = new Color(1, 0, 0, 0);
        } 
        pcStoryBoardParent.gameObject.SetActive(false);

        _vrStoryBoard = new SpriteRenderer[vrStoryBoardParent.childCount];
        for (int i = 0; i < vrStoryBoardParent.childCount; i++)
        {
            _vrStoryBoard[i] = vrStoryBoardParent.GetChild(i).GetComponent<SpriteRenderer>();
            _vrStoryBoard[i].color = new Color(1, 0, 0, 0);
        } 
        vrStoryBoardParent.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        EventsManager.OnStartStoryBoard += StartStoryBoard;
    }

    void OnDisable()
    {
        EventsManager.OnStartStoryBoard -= StartStoryBoard;
    }

    //Can be used by UI Buttons
    public void StartStoryBoard()
    {
        EventsManager.SetMenu(-1);

        pcStoryBoardParent.gameObject.SetActive(true);
        vrStoryBoardParent.gameObject.SetActive(true);

        _playStoryBoard = true;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ShowStoryRpc(float _delay, int _storyNum)
    {
        StartCoroutine(ShowStory(1f, _currentStory));   
    }

    IEnumerator ShowStory(float _delay, int _storyNum)
    {
        _audioSource.clip = _storyBoardAudioClips[_storyNum];
        _audioSource.Play(); 

        EventsManager.ReduceBackgroundMusic(0.75f);

        float elapsed = 0f;
        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            _pcStoryBoard[_storyNum].color = new Color(1, 0, 0, elapsed / _delay);
            _vrStoryBoard[_storyNum].color = new Color(1, 0, 0, elapsed / _delay);
            yield return null;
        }

        _pcStoryBoard[_storyNum].color = new Color(1, 0, 0, 1);
        _vrStoryBoard[_storyNum].color = new Color(1, 0, 0, 1);

        _currentStory++;
    }

    IEnumerator HideStories(float _delay)
    {
        _audioSource.clip = _storyBoardAudioClips[pcStoryBoardParent.childCount];
        _audioSource.Play(); 

        EventsManager.ReduceBackgroundMusic(0f);

        float elapsed = 0f;
        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < pcStoryBoardParent.childCount; i++)
            {
                _pcStoryBoard[i].color = new Color(1, 0, 0, 1f - (elapsed / _delay));
                _vrStoryBoard[i].color = new Color(1, 0, 0, 1f - (elapsed / _delay));
            } 

            yield return null;
        }

        for (int i = 0; i < pcStoryBoardParent.childCount; i++)
        {
            _pcStoryBoard[i].color = new Color(1, 0, 0, 0f);
            _vrStoryBoard[i].color = new Color(1, 0, 0, 0f);
        } 

        _currentStory++;
    }

    private void Update()
    {
        if(!_playStoryBoard || _audioSource.isPlaying) return;

        if(_currentStory > _maxStoryNum)
        {
            _playStoryBoard = false;
            EventsManager.PlayLevel("PlayLevelScene");
        }else if(_currentStory == _maxStoryNum){
            StartCoroutine(HideStories(1f));   
        }else{
            StartCoroutine(ShowStory(1f, _currentStory));   
        }
    }
}
