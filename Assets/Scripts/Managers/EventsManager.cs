using UnityEngine;
using UnityEngine.InputSystem;
using System;

using Interhaptics;
using Interhaptics.Utils;

public class EventsManager : MonoBehaviour
{
    //Debug Events=============================================
    public static event Action OnUsingPlayMode;
    public static void UsingPlayMode() => OnUsingPlayMode?.Invoke();
    public static event Action OnUsingOnlyVRPlayer;
    public static void UsingOnlyVRPlayer() => OnUsingOnlyVRPlayer?.Invoke();
    public static event Action OnUsingOnlyPCPlayer;
    public static void UsingOnlyPCPlayer() => OnUsingOnlyPCPlayer?.Invoke();

    public static event Action OnSkipTutorial;
    public static void SkipTutorial() => OnSkipTutorial?.Invoke();

    public static event Action OnActivateSpinWheel;
    public static void ActivateSpinWheel() => OnActivateSpinWheel?.Invoke();
    public static event Action OnActivateHealthBall;
    public static void ActivateHealthBall() => OnActivateHealthBall?.Invoke();
    public static event Action OnActivateDefenseButton;
    public static void ActivateDefenseButton() => OnActivateDefenseButton?.Invoke();
    public static event Action<int> OnPressedDefenseButton;
    public static void PressedDefenseButton(int _enemyCount) => OnPressedDefenseButton?.Invoke(_enemyCount);

    public static event Action OnDisableEnemySpawning;
    public static void DisableEnemySpawning() => OnDisableEnemySpawning?.Invoke();

    public static event Action OnDisableTeleportChange;
    public static void DisableTeleportChange() => OnDisableTeleportChange?.Invoke();




    //Network Events=============================================
    public static event Action OnClientButton;
    public static void ClientButton() => OnClientButton?.Invoke();
    public static event Action OnHostButton;
    public static void HostButton() => OnHostButton?.Invoke();

    public static event Action OnCreatedPCPlayer;
    public static void CreatedPCPlayer() => OnCreatedPCPlayer?.Invoke();
    public static event Action OnAddPCPlayerBody;
    public static void AddPCPlayerBody() => OnAddPCPlayerBody?.Invoke();
    public static event Action OnCreatedPCPlayerBody;
    public static void CreatedPCPlayerBody() => OnCreatedPCPlayerBody?.Invoke();
    public static event Action OnCreatedVRPlayer;
    public static void CreatedVRPlayer() => OnCreatedVRPlayer?.Invoke();
    
    public static event Action<ulong> OnSetPCPlayerID;
    public static void SetPCPlayerID(ulong _clientID) => OnSetPCPlayerID?.Invoke(_clientID);
    public static event Action<ulong> OnSetVRPlayerID;
    public static void SetVRPlayerID(ulong _clientID) => OnSetVRPlayerID?.Invoke(_clientID);





    //BackgroundMusic Events=============================================
    public static event Action OnPlayFirstSectionMusic;
    public static void PlayFirstSectionMusic() => OnPlayFirstSectionMusic?.Invoke();
    public static event Action OnPlaySecondSectionMusic;
    public static void PlaySecondSectionMusic() => OnPlaySecondSectionMusic?.Invoke();
    public static event Action OnPlayThirdSectionMusic;
    public static void PlayThirdSectionMusic() => OnPlayThirdSectionMusic?.Invoke();
    public static event Action OnPlayFourthSectionMusic;
    public static void PlayFourthSectionMusic() => OnPlayFourthSectionMusic?.Invoke();





    //GameMenu Events=============================================
    public static event Action OnPlayGameLevel;
    public static void PlayGameLevel() => OnPlayGameLevel?.Invoke();
    public static event Action OnPlayMainMenuLevel;
    public static void PlayMainMenuLevel() => OnPlayMainMenuLevel?.Invoke();

    public static event Action OnChangeLanguageToEnglish;
    public static void ChangeLanguageToEnglish() => OnChangeLanguageToEnglish?.Invoke();
    public static event Action OnChangeLanguageToFrench;
    public static void ChangeLanguageToFrench() => OnChangeLanguageToFrench?.Invoke();
    
    public static event Action<bool> OnCancel;
    public static void Cancel(bool _toggle) => OnCancel?.Invoke(_toggle);
    public static event Action<bool> OnSettingsMenu;
    public static void SettingsMenu(bool _toggle) => OnSettingsMenu?.Invoke(_toggle);

    public static event Action<bool> OnGameOver;
    public static void GameOver(bool _toggle) => OnGameOver?.Invoke(_toggle);
    public static event Action<bool> OnToggleAll;
    public static void ToggleAll(bool _toggle) => OnToggleAll?.Invoke(_toggle);
    public static event Action<string, bool> OnToggleRestriction;
    public static void ToggleRestriction(string _restriction, bool _toggle) => OnToggleRestriction?.Invoke(_restriction, _toggle);
    public static event Action<bool> OnFreezePCPlayer;
    public static void FreezePCPlayer(bool _toggle) => OnFreezePCPlayer?.Invoke(_toggle);


    public static event Action<bool> OnEnteredTemple;
    public static void EnteredTemple(bool _toggle) => OnEnteredTemple?.Invoke(_toggle);
    public static event Action<float> OnChangeHealthBar;
    public static void ChangeHealthBar(float _newHealth) => OnChangeHealthBar?.Invoke(_newHealth);
    public static event Action<float> OnChangeChargeBar;
    public static void ChangeChargeBar(float _newCharge) => OnChangeChargeBar?.Invoke(_newCharge);





    //MainMenu Events=============================================
    public static event Action OnChosePlayer;
    public static void ChosePlayer() => OnChosePlayer?.Invoke();

    public static event Action<int> OnSetMenuWithoutNetwork;
    public static void SetMenuWithoutNetwork(int _menuNum) => OnSetMenuWithoutNetwork?.Invoke(_menuNum);
    public static event Action<int> OnSetMenu;
    public static void SetMenu(int _menuNum) => OnSetMenu?.Invoke(_menuNum);

    public static event Action<string> OnPlayLevel;
    public static void PlayLevel(string _sceneName) => OnPlayLevel?.Invoke(_sceneName);
    public static event Action OnQuit;
    public static void Quit() => OnQuit?.Invoke();

    public static event Action OnRevealTemple;
    public static void RevealTemple() => OnRevealTemple?.Invoke();

    public static event Action<int> OnChangeLanguage;
    public static void ChangeLanguage(int _languageNum) => OnChangeLanguage?.Invoke(_languageNum);

    public static event Action<float> OnReduceBackgroundMusic;
    public static void ReduceBackgroundMusic(float _reducedAmount) => OnReduceBackgroundMusic?.Invoke(_reducedAmount);

    





    //Enemy Events=============================================
    public static event Action OnDestroyAllEnemies;
    public static void DestroyAllEnemies() => OnDestroyAllEnemies?.Invoke();
    public static event Action<float> OnIncreaseChanceOfSpawningEnemy;
    public static void IncreaseChanceOfSpawningEnemy(float _chance) => OnIncreaseChanceOfSpawningEnemy?.Invoke(_chance);
    public static event Action<int> OnCreateRandomEnemy;
    public static void CreateRandomEnemy(int _enemyNum) => OnCreateRandomEnemy?.Invoke(_enemyNum);
    public static event Action<GameObject> OnRemoveEnemy;
    public static void RemoveEnemy(GameObject _enemy) => OnRemoveEnemy?.Invoke(_enemy);
    
    public static event Action<GameObject, int> OnGetAppropriateEnemyAudio;
    public static void GetAppropriateEnemyAudio(GameObject _enemy, int _audioType) => OnGetAppropriateEnemyAudio?.Invoke(_enemy, _audioType);
    public static event Action<GameObject, AudioClip, int> OnSendAppropriateEnemyAudio;
    public static void SendAppropriateEnemyAudio(GameObject _enemy, AudioClip _audio, int _audioNum) => OnSendAppropriateEnemyAudio?.Invoke(_enemy, _audio, _audioNum);

    public static event Action OnDisableEnemies;
    public static void DisableEnemies() => OnDisableEnemies?.Invoke();





    //Reset Events=============================================
    public static event Action OnResetHiddenSwitches;
    public static void ResetHiddenSwitches() => OnResetHiddenSwitches?.Invoke();
    public static event Action OnResetHiddenButtons;

    public static event Action OnTrueResetButtons;
    public static void TrueResetButtons() => OnTrueResetButtons?.Invoke();
    public static event Action OnResetButtons;
    public static void ResetButtons() => OnResetButtons?.Invoke();
    public static event Action<ShapeType> OnFreezeCorrectButtons;
    public static void FreezeCorrectButtons(ShapeType _shape) => OnFreezeCorrectButtons?.Invoke(_shape);

    public static void ResetHiddenButtons() => OnResetHiddenButtons?.Invoke();
    public static event Action OnResetTeleportPads;
    public static void ResetTeleportPads() => OnResetTeleportPads?.Invoke();
    public static event Action<GameObject> OnResetHealth;
    public static void ResetHealth(GameObject _entity) => OnResetHealth?.Invoke(_entity);





    //Switch Events=============================================
    public enum ShapeType { None, Health, Spin, Defense }
    public enum ButtonType { None, Square, Circle, Triangle, Cross, Star }
    public static event Action<ShapeType, ButtonType> OnTriggerButton;
    public static void TriggerButton(ShapeType _shape, ButtonType _button) => OnTriggerButton?.Invoke(_shape, _button);
    public static event Action OnActivateReset;
    public static void ActivateReset() => OnActivateReset?.Invoke();
    public static event Action OnGivePCPlayerHealth;
    public static void GivePCPlayerHealth() => OnGivePCPlayerHealth?.Invoke();





    //Health Events=============================================
    public static event Action<float> OnChangeHealthCamera;
    public static void ChangeHealthCamera(float _healthCameraIntensity) => OnChangeHealthCamera?.Invoke(_healthCameraIntensity);
    public static event Action<GameObject, float> OnChangeHealthForEntity;
    public static void ChangeHealthForEntity(GameObject _entity, float _newHealth) => OnChangeHealthForEntity?.Invoke(_entity, _newHealth);
    public static event Action<GameObject, float> OnEntityChangedHealth;
    public static void EntityChangedHealth(GameObject _entity, float _newHealth) => OnEntityChangedHealth?.Invoke(_entity, _newHealth);
    public static event Action<GameObject> OnEntityKilled;
    public static void EntityKilled(GameObject _entity) => OnEntityKilled?.Invoke(_entity);
    public static event Action<Renderer, float> OnChangedEnemyOxidization;
    public static void ChangedEnemyOxidization(Renderer _rend, float _oxidization) => OnChangedEnemyOxidization?.Invoke(_rend, _oxidization);





    //Hidden Object Events=============================================
    public static event Action OnTriggerHiddenButton;
    public static void TriggerHiddenButton() => OnTriggerHiddenButton?.Invoke();
    public static event Action<GameObject> OnRemoveHiddenObject;
    public static void RemoveHiddenObject(GameObject _hiddenObject) => OnRemoveHiddenObject?.Invoke(_hiddenObject);
    public static event Action<GameObject, bool, bool, bool, bool> OnAddNewHiddenObject;
    public static void AddNewHiddenObject(  GameObject _hiddenObject, 
                                            bool _isPCInteractable, 
                                            bool _isVRInteractable, 
                                            bool _isEffectedByLight,
                                            bool _isReverse) => OnAddNewHiddenObject?.Invoke(_hiddenObject, 
                                                                                            _isPCInteractable, 
                                                                                            _isVRInteractable, 
                                                                                            _isEffectedByLight, 
                                                                                            _isReverse);
    public static event Action<GameObject> OnAddSpecificHiddenObject;
    public static void AddSpecificHiddenObject(GameObject _hiddenObject) => OnAddSpecificHiddenObject?.Invoke(_hiddenObject);
    





    //Teleporter Events=============================================
    public static event Action<int> OnSendCodeToTeleportPads;
    public static void SendCodeToTeleportPads(int _code) => OnSendCodeToTeleportPads?.Invoke(_code);
    public static event Action<float, float> OnChangeTeleportRotateSpeed;
    public static void ChangeTeleportRotateSpeed(float _currentRotateSpeed, float _maxRotateSpeed) => OnChangeTeleportRotateSpeed?.Invoke(_currentRotateSpeed, _maxRotateSpeed);
    public static event Action<float, bool> OnFixTeleportEffect;
    public static void FixTeleportEffect(float _teleportEffect, bool _canChange) => OnFixTeleportEffect?.Invoke(_teleportEffect, _canChange);
    public static event Action<Transform> OnAddNewBar;
    public static void AddNewBar(Transform _bar) => OnAddNewBar?.Invoke(_bar);
    public static event Action<bool> OnChangePadsReady;
    public static void ChangePadsReady(bool _newPadsReady) => OnChangePadsReady?.Invoke(_newPadsReady);
    public static event Action<GameObject> OnTriggerTeleportButton;
    public static void TriggerTeleportButton(GameObject _teleportPad) => OnTriggerTeleportButton?.Invoke(_teleportPad);
    public static event Action<GameObject, Transform> OnSetExitPadTransform;
    public static void SetExitPadTransform(GameObject _teleportPad, Transform _exitPad) => OnSetExitPadTransform?.Invoke(_teleportPad, _exitPad);
    public static event Action OnTutorialTeleport;
    public static void TutorialTeleport() => OnTutorialTeleport?.Invoke();

    public static event Action<bool> OnTogglePCTrigger;
    public static void TogglePCTrigger(bool _toggle) => OnTogglePCTrigger?.Invoke(_toggle);

    public static event Action OnClearTeleportNumPad;
    public static void ClearTeleportNumPad() => OnClearTeleportNumPad?.Invoke();

    public static event Action<float> OnChangeHidingBarPosition;
    public static void ChangeHidingBarPosition(float _position) => OnChangeHidingBarPosition?.Invoke(_position);
    public static event Action OnUpdateHidingBar;
    public static void UpdateHidingBar() => OnUpdateHidingBar?.Invoke();
    public static event Action<Transform> OnTriggerRopeButton;
    public static void TriggerRopeButton(Transform _button) => OnTriggerRopeButton?.Invoke(_button);






    //Respawn Events=============================================
    public static event Action OnRespawn;
    public static void Respawn() => OnRespawn?.Invoke();
    public static event Action<GameObject> OnObjectRespawned;
    public static void ObjectRespawned(GameObject _entity) => OnObjectRespawned?.Invoke(_entity);





    //PC Tutorial Events=============================================
    public static event Action OnTriggerPCChargeTutorial;
    public static void TriggerPCChargeTutorial() => OnTriggerPCChargeTutorial?.Invoke();
    public static event Action OnTriggerPCInteractTutorial;
    public static void TriggerPCInteractTutorial() => OnTriggerPCInteractTutorial?.Invoke();
    public static event Action OnResetPCTutorial;
    public static void ResetPCTutorial() => OnResetPCTutorial?.Invoke();

    



    //Plot Events=============================================
    public static event Action OnReachedSwitches;
    public static void ReachedSwitches() => OnReachedSwitches?.Invoke();
    public static event Action OnFirstEnemyCreated;
    public static void FirstEnemyCreated() => OnFirstEnemyCreated?.Invoke();
    public static event Action OnFirstActiveInteractiveObject;
    public static void FirstActiveInteractiveObject() => OnFirstActiveInteractiveObject?.Invoke();
    
    public static event Action OnReachedFirstTeleporter;
    public static void ReachedFirstTeleporter() => OnReachedFirstTeleporter?.Invoke();
    public static event Action OnFirstCollectable;
    public static void FirstCollectable() => OnFirstCollectable?.Invoke();

    public static event Action OnUsedCrookedBridgeTeleporter;
    public static void UsedCrookedBridgeTeleporter() => OnUsedCrookedBridgeTeleporter?.Invoke();




    //Progress Events=============================================
    public static event Action OnEverythingCollected;
    public static void EverythingCollected() => OnEverythingCollected?.Invoke();
    public static event Action OnCrossedCrookedBridges;
    public static void CrossedCrookedBridges() => OnCrossedCrookedBridges?.Invoke();





    //Checkpoints Events=============================================
    public static event Action OnActivateSecondCheckpointHiddenObjects;
    public static void ActivateSecondCheckpointHiddenObjects() => OnActivateSecondCheckpointHiddenObjects?.Invoke();
    public static event Action OnActivateThirdCheckpointHiddenObjects;    
    public static void ActivateThirdCheckpointHiddenObjects() => OnActivateThirdCheckpointHiddenObjects?.Invoke();





    //Controllers Events=============================================
    public static event Action OnChangedControllers;
    public static void ChangedControllers() => OnChangedControllers?.Invoke();
    public static event Action OnCheckControllers;
    public static void CheckControllers() => OnCheckControllers?.Invoke();
    public static event Action<int> OnActivateController;
    public static void ActivateController(int controllerNum) => OnActivateController?.Invoke(controllerNum);
    public static event Action<int> OnDeactivateController;
    public static void DeactivateController(int controllerNum) => OnDeactivateController?.Invoke(controllerNum);
    public static event Action OnUpdateVRFlashlight;
    public static void UpdateVRFlashlight() => OnUpdateVRFlashlight?.Invoke();





    //PC Player Input Events=============================================
    public static event Action<Vector2> OnMove;
    public static void Move(Vector2 movement) => OnMove?.Invoke(movement);
    public static event Action OnJump;
    public static void Jump() => OnJump?.Invoke();
    public static event Action OnInteract;
    public static void Interact() => OnInteract?.Invoke();
    public static event Action<InputAction.CallbackContext> OnFire;
    public static void Fire(InputAction.CallbackContext ctx) => OnFire?.Invoke(ctx);
    public static event Action OnFire2;
    public static void Fire2() => OnFire2?.Invoke();

    public static event Action<float> OnEntityChangedFlashlightRange;
    public static void EntityChangedFlashlightRange(float newFlashlightRange) => OnEntityChangedFlashlightRange?.Invoke(newFlashlightRange);





    //NarratorManager Events=============================================
    public static event Action<string, string, bool, int, bool> OnTriggerNarratorAudio;
    public static void TriggerNarratorAudio(string _sectionName, string _audioName, bool _isVRPlayer, int _lookAt, bool _stay) => OnTriggerNarratorAudio?.Invoke(_sectionName, _audioName, _isVRPlayer, _lookAt, _stay);
    public static event Action OnNarratorStopped;
    public static void NarratorStopped() => OnNarratorStopped?.Invoke();
    public static event Action<bool> OnTogglePauseManagerAudio;
    public static void TogglePauseManagerAudio(bool _toggle) => OnTogglePauseManagerAudio?.Invoke(_toggle);

    public static event Action OnUseEnglishNarrator;
    public static void UseEnglishNarrator() => OnUseEnglishNarrator?.Invoke();
    public static event Action OnUseFrenchNarrator;
    public static void UseFrenchNarrator() => OnUseFrenchNarrator?.Invoke();

    public static event Action<int, int, bool> OnLookAtPlayer;
    public static void LookAtPlayer(int _playerNum, int _lookAt, bool _stay) => OnLookAtPlayer?.Invoke(_playerNum, _lookAt, _stay);
    public static event Action<AudioClip, AudioHapticSource> OnNarratorSays;
    public static void NarratorSays(AudioClip _audioClip, AudioHapticSource _hapticSource) => OnNarratorSays?.Invoke(_audioClip, _hapticSource);





    //VR Teleport Table Events=============================================
    public static event Action<int> OnAddSymbolNumber;
    public static void AddSymbolNumber(int _number) => OnAddSymbolNumber?.Invoke(_number);
    public static event Action OnRemoveSymbol;
    public static void RemoveSymbol() => OnRemoveSymbol?.Invoke();
    public static event Action OnInputSymbols;
    public static void InputSymbols() => OnInputSymbols?.Invoke();

    public static event Action<int> OnActivateLever;
    public static void ActivateLever(int _leverIndex) => OnActivateLever?.Invoke(_leverIndex);
    public static event Action<int> OnDeactivateLever;
    public static void DeactivateLever(int _leverIndex) => OnDeactivateLever?.Invoke(_leverIndex);





    //Ending Events=============================================





    //StoryBoard Events=============================================
    public static event Action OnStartStoryBoard;
    public static void StartStoryBoard() => OnStartStoryBoard?.Invoke();





    //Haptic Events=============================================
    public static event Action<int> OnPingVRController;
    public static void PingVRController(int _controllerNum) => OnPingVRController?.Invoke(_controllerNum);
    public static event Action<bool> OnUseEnemyHaptic;
    public static void UseEnemyHaptic(bool _useHaptic) => OnUseEnemyHaptic?.Invoke(_useHaptic);
    public static event Action<float> OnUseBridgeHaptic;
    public static void UseBridgeHaptic(float _bridgeMovementAmount) => OnUseBridgeHaptic?.Invoke(_bridgeMovementAmount);
    public static event Action<int, float> OnUseRopeHaptic;
    public static void UseRopeHaptic(int _controllerNum, float _ropeDistance) => OnUseRopeHaptic?.Invoke(_controllerNum, _ropeDistance);

    public static event Action<float> OnUseTeleportBarHaptic;
    public static void UseTeleportBarHaptic(float _intensity) => OnUseTeleportBarHaptic?.Invoke(_intensity);
    public static event Action<int> OnUseButtonHaptic;
    public static void UseButtonHaptic(int _buttonNum) => OnUseButtonHaptic?.Invoke(_buttonNum);
    public static event Action OnUseAllButtonHaptic;
    public static void UseAllButtonHaptic() => OnUseAllButtonHaptic?.Invoke();

    public static event Action<bool> OnIsNarratorSpeaking;
    public static void IsNarratorSpeaking(bool _isNarratorSpeaking) => OnIsNarratorSpeaking?.Invoke(_isNarratorSpeaking);

    public static event Action OnActivateTeleporterTimerHaptic;
    public static void ActivateTeleporterTimerHaptic() => OnActivateTeleporterTimerHaptic?.Invoke();
    public static event Action<float> OnTeleporterTransitionHaptic;
    public static void TeleporterTransitionHaptic(float _newIntensity) => OnTeleporterTransitionHaptic?.Invoke(_newIntensity);

    public static event Action<float> OnChangeHealthHaptic;
    public static void ChangeHealthHaptic(float _newHealth) => OnChangeHealthHaptic?.Invoke(_newHealth);

    public static event Action<AudioHapticSource> OnChangeNarratorHaptic;
    public static void ChangeNarratorHaptic(AudioHapticSource _newHapticSource) => OnChangeNarratorHaptic?.Invoke(_newHapticSource);
    public static event Action OnPlayNarratorHaptic;
    public static void PlayNarratorHaptic() => OnPlayNarratorHaptic?.Invoke();
    public static event Action OnStopNarratorHaptic;
    public static void StopNarratorHaptic() => OnStopNarratorHaptic?.Invoke();

    public static event Action<AudioHapticSource> OnPlayStoneButtonHaptic;
    public static void PlayStoneButtonHaptic(AudioHapticSource _haptic) => OnPlayStoneButtonHaptic?.Invoke(_haptic);

    public static event Action<EventsManager.ButtonType> OnPressedButtonHaptic;
    public static void PressedButtonHaptic(EventsManager.ButtonType _button) => OnPressedButtonHaptic?.Invoke(_button);
}
