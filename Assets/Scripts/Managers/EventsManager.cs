using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class EventsManager : MonoBehaviour
{
    //Network Events=============================================
    public static event Action OnCreatedPCPlayer;
    public static void CreatedPCPlayer() => OnCreatedPCPlayer?.Invoke();
    public static event Action OnAddPCPlayerBody;
    public static void AddPCPlayerBody() => OnAddPCPlayerBody?.Invoke();
    public static event Action OnCreatedPCPlayerBody;
    public static void CreatedPCPlayerBody() => OnCreatedPCPlayerBody?.Invoke();
    public static event Action OnCreatedVRPlayer;
    public static void CreatedVRPlayer() => OnCreatedVRPlayer?.Invoke();





    //GameOver Events=============================================
    public static event Action OnGameOver;
    public static void GameOver() => OnGameOver?.Invoke();
    public static event Action<bool> OnToggleAll;
    public static void ToggleAll(bool toggle) => OnToggleAll?.Invoke(toggle);





    //Enemy Events=============================================
    public static event Action OnDisableEnemySpawning;
    public static void DisableEnemySpawning() => OnDisableEnemySpawning?.Invoke();
    public static event Action OnDestroyAllEnemies;
    public static void DestroyAllEnemies() => OnDestroyAllEnemies?.Invoke();
    public static event Action<float> OnIncreaseChanceOfSpawningEnemy;
    public static void IncreaseChanceOfSpawningEnemy(float chance) => OnIncreaseChanceOfSpawningEnemy?.Invoke(chance);
    public static event Action<int> OnCreateRandomEnemy;
    public static void CreateRandomEnemy(int enemyNum) => OnCreateRandomEnemy?.Invoke(enemyNum);
    public static event Action<GameObject> OnRemoveEnemy;
    public static void RemoveEnemy(GameObject enemy) => OnRemoveEnemy?.Invoke(enemy);
    public static event Action<GameObject, int> OnGetAppropriateEnemyAudio;

    public static void GetAppropriateEnemyAudio(GameObject enemy, int audioType) => OnGetAppropriateEnemyAudio?.Invoke(enemy, audioType);
    public static event Action<GameObject, AudioClip> OnSendAppropriateEnemyAudio;
    public static void SendAppropriateEnemyAudio(GameObject enemy, AudioClip audio) => OnSendAppropriateEnemyAudio?.Invoke(enemy, audio);





    //Reset Events=============================================
    public static event Action OnResetHiddenSwitches;
    public static void ResetHiddenSwitches() => OnResetHiddenSwitches?.Invoke();
    public static event Action OnResetHiddenButtons;
    public static event Action OnResetButtons;
    public static void ResetButtons() => OnResetButtons?.Invoke();
    public static void ResetHiddenButtons() => OnResetHiddenButtons?.Invoke();
    public static event Action OnResetTeleportPads;
    public static void ResetTeleportPads() => OnResetTeleportPads?.Invoke();
    public static event Action<GameObject> OnResetHealth;
    public static void ResetHealth(GameObject entity) => OnResetHealth?.Invoke(entity);





    //Switch Events=============================================
    public enum ShapeType { None, Health, Spin, Defense }
    public enum ButtonType { None, Square, Circle, Triangle, Cross, Star }
    public static event Action<ShapeType, ButtonType> OnTriggerButton;
    public static void TriggerButton(ShapeType shape, ButtonType button) => OnTriggerButton?.Invoke(shape, button);
    public static event Action OnActivateReset;
    public static void ActivateReset() => OnActivateReset?.Invoke();
    public static event Action OnGivePCPlayerHealth;
    public static void GivePCPlayerHealth() => OnGivePCPlayerHealth?.Invoke();





    //Health Events=============================================
    public static event Action<float> OnChangeHealthCamera;
    public static void ChangeHealthCamera(float healthCameraIntensity) => OnChangeHealthCamera?.Invoke(healthCameraIntensity);
    public static event Action<float> OnChangeHealthBar;
    public static void ChangeHealthBar(float newHealth) => OnChangeHealthBar?.Invoke(newHealth);
    public static event Action<GameObject, float> OnChangeHealthForEntity;
    public static void ChangeHealthForEntity(GameObject entity, float newHealth) => OnChangeHealthForEntity?.Invoke(entity, newHealth);
    public static event Action<GameObject, float> OnEntityChangedHealth;
    public static void EntityChangedHealth(GameObject entity, float newHealth) => OnEntityChangedHealth?.Invoke(entity, newHealth);
    public static event Action<GameObject> OnEntityKilled;
    public static void EntityKilled(GameObject entity) => OnEntityKilled?.Invoke(entity);
    public static event Action<Renderer, float> OnChangedEnemyOxidization;
    public static void ChangedEnemyOxidization(Renderer rend, float oxidization) => OnChangedEnemyOxidization?.Invoke(rend, oxidization);





    //Charge Events=============================================
    public static event Action<float> OnChangeChargeBar;
    public static void ChangeChargeBar(float newCharge) => OnChangeChargeBar?.Invoke(newCharge);





    //Hidden Object Events=============================================
    public static event Action OnTriggerHiddenButton;
    public static void TriggerHiddenButton() => OnTriggerHiddenButton?.Invoke();
    public static event Action<GameObject> OnRemoveHiddenObject;
    public static void RemoveHiddenObject(GameObject hiddenObject) => OnRemoveHiddenObject?.Invoke(hiddenObject);
    public static event Action<GameObject, bool, bool, bool, bool> OnAddNewHiddenObject;
    public static void AddNewHiddenObject(  GameObject hiddenObject, 
                                            bool isPCInteractable, 
                                            bool isVRInteractable, 
                                            bool isEffectedByLight,
                                            bool isReverse) => OnAddNewHiddenObject?.Invoke(hiddenObject, 
                                                                                            isPCInteractable, 
                                                                                            isVRInteractable, 
                                                                                            isEffectedByLight, 
                                                                                            isReverse);





    //Teleporter Events=============================================
    public static event Action<int> OnSendCodeToTeleportPads;
    public static void SendCodeToTeleportPads(int code) => OnSendCodeToTeleportPads?.Invoke(code);
    public static event Action<float, float> OnChangeTeleportRotateSpeed;
    public static void ChangeTeleportRotateSpeed(float currentRotateSpeed, float maxRotateSpeed) => OnChangeTeleportRotateSpeed?.Invoke(currentRotateSpeed, maxRotateSpeed);
    public static event Action<Transform> OnAddNewBar;
    public static void AddNewBar(Transform bar) => OnAddNewBar?.Invoke(bar);
    public static event Action<bool> OnChangePadsReady;
    public static void ChangePadsReady(bool _newPadsReady) => OnChangePadsReady?.Invoke(_newPadsReady);
    public static event Action<GameObject> OnTriggerTeleportButton;
    public static void TriggerTeleportButton(GameObject _teleportPad) => OnTriggerTeleportButton?.Invoke(_teleportPad);






    //Respawn Events=============================================
    public static event Action<GameObject> OnObjectRespawned;
    public static void ObjectRespawned(GameObject entity) => OnObjectRespawned?.Invoke(entity);





    //PC Tutorial Events=============================================
    public static event Action OnTriggerPCChargeTutorial;
    public static void TriggerPCChargeTutorial() => OnTriggerPCChargeTutorial?.Invoke();
    public static event Action OnTriggerPCInteractTutorial;
    public static void TriggerPCInteractTutorial() => OnTriggerPCInteractTutorial?.Invoke();




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





    //PC Player Input=============================================
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
    public static event Action OnCancel;
    public static void Cancel() => OnCancel?.Invoke();

    public static event Action<float> OnEntityChangedFlashlightRange;
    public static void EntityChangedFlashlightRange(float newFlashlightRange) => OnEntityChangedFlashlightRange?.Invoke(newFlashlightRange);

}
