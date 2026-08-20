using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class EventsManager : MonoBehaviour
{
    //Debug Events=============================================
    public static event Action OnUsingPlayMode;
    public static void UsingPlayMode() => OnUsingPlayMode?.Invoke();
    public static event Action OnUsingOnlyVRPlayer;
    public static void UsingOnlyVRPlayer() => OnUsingOnlyVRPlayer?.Invoke();
    public static event Action OnUsingOnlyPCPlayer;
    public static void UsingOnlyPCPlayer() => OnUsingOnlyPCPlayer?.Invoke();

    public static event Action OnActivateSpinWheel;
    public static void ActivateSpinWheel() => OnActivateSpinWheel?.Invoke();
    public static event Action OnActivateHealthBall;
    public static void ActivateHealthBall() => OnActivateHealthBall?.Invoke();
    public static event Action OnActivateDefenseButton;
    public static void ActivateDefenseButton() => OnActivateDefenseButton?.Invoke();




    //Network Events=============================================
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





    //GameOver Events=============================================
    public static event Action<bool> OnGameOver;
    public static void GameOver(bool _toggle) => OnGameOver?.Invoke(_toggle);
    public static event Action<bool> OnToggleAll;
    public static void ToggleAll(bool _toggle) => OnToggleAll?.Invoke(_toggle);
    public static event Action<string, bool> OnToggleRestriction;
    public static void ToggleRestriction(string _restriction, bool _toggle) => OnToggleRestriction?.Invoke(_restriction, _toggle);

    public static event Action<bool> OnFreezePCPlayer;
    public static void FreezePCPlayer(bool _toggle) => OnFreezePCPlayer?.Invoke(_toggle);





    //Enemy Events=============================================
    public static event Action OnDisableEnemySpawning;
    public static void DisableEnemySpawning() => OnDisableEnemySpawning?.Invoke();
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
    public static event Action<GameObject, AudioClip> OnSendAppropriateEnemyAudio;
    public static void SendAppropriateEnemyAudio(GameObject _enemy, AudioClip _audio) => OnSendAppropriateEnemyAudio?.Invoke(_enemy, _audio);

    public static event Action OnDisableEnemies;
    public static void DisableEnemies() => OnDisableEnemies?.Invoke();

    public static event Action OnDisableTeleportChange;
    public static void DisableTeleportChange() => OnDisableTeleportChange?.Invoke();





    //Reset Events=============================================
    public static event Action OnResetHiddenSwitches;
    public static void ResetHiddenSwitches() => OnResetHiddenSwitches?.Invoke();
    public static event Action OnResetHiddenButtons;

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
    public static event Action<float> OnChangeHealthBar;
    public static void ChangeHealthBar(float _newHealth) => OnChangeHealthBar?.Invoke(_newHealth);
    public static event Action<GameObject, float> OnChangeHealthForEntity;
    public static void ChangeHealthForEntity(GameObject _entity, float _newHealth) => OnChangeHealthForEntity?.Invoke(_entity, _newHealth);
    public static event Action<GameObject, float> OnEntityChangedHealth;
    public static void EntityChangedHealth(GameObject _entity, float _newHealth) => OnEntityChangedHealth?.Invoke(_entity, _newHealth);
    public static event Action<GameObject> OnEntityKilled;
    public static void EntityKilled(GameObject _entity) => OnEntityKilled?.Invoke(_entity);
    public static event Action<Renderer, float> OnChangedEnemyOxidization;
    public static void ChangedEnemyOxidization(Renderer _rend, float _oxidization) => OnChangedEnemyOxidization?.Invoke(_rend, _oxidization);





    //Charge Events=============================================
    public static event Action<float> OnChangeChargeBar;
    public static void ChangeChargeBar(float _newCharge) => OnChangeChargeBar?.Invoke(_newCharge);





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






    //Respawn Events=============================================
    public static event Action<GameObject> OnObjectRespawned;
    public static void ObjectRespawned(GameObject _entity) => OnObjectRespawned?.Invoke(_entity);





    //PC Tutorial Events=============================================
    public static event Action OnTriggerPCChargeTutorial;
    public static void TriggerPCChargeTutorial() => OnTriggerPCChargeTutorial?.Invoke();
    public static event Action OnTriggerPCInteractTutorial;
    public static void TriggerPCInteractTutorial() => OnTriggerPCInteractTutorial?.Invoke();
    public static event Action OnResetPCTutorial;
    public static void ResetPCTutorial() => OnResetPCTutorial?.Invoke();
    
    




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
    public static event Action<bool> OnCancel;
    public static void Cancel(bool _toggle) => OnCancel?.Invoke(_toggle);

    public static event Action<float> OnEntityChangedFlashlightRange;
    public static void EntityChangedFlashlightRange(float newFlashlightRange) => OnEntityChangedFlashlightRange?.Invoke(newFlashlightRange);





    //NarratorManager Events=============================================
    public static event Action<string, string, bool> OnTriggerNarratorAudio;
    public static void TriggerNarratorAudio(string _sectionName, string _audioName, bool _isVRPlayer) => OnTriggerNarratorAudio?.Invoke(_sectionName, _audioName, _isVRPlayer);
    public static event Action OnNarratorStopped;
    public static void NarratorStopped() => OnNarratorStopped?.Invoke();
    public static event Action<bool> OnTogglePauseManagerAudio;
    public static void TogglePauseManagerAudio(bool _toggle) => OnTogglePauseManagerAudio?.Invoke(_toggle);

    public static event Action OnUseEnglishNarrator;
    public static void UseEnglishNarrator() => OnUseEnglishNarrator?.Invoke();
    public static event Action OnUseFrenchNarrator;
    public static void UseFrenchNarrator() => OnUseFrenchNarrator?.Invoke();
    





    //Ending Events=============================================
    public static event Action<bool> OnEnteredTemple;
    public static void EnteredTemple(bool _toggle) => OnEnteredTemple?.Invoke(_toggle);
}
