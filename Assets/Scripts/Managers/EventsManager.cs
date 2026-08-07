using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class EventsManager : MonoBehaviour
{
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

    //Health Events=============================================
    public static event Action<float> OnChangeHealthCamera;
    public static void ChangeHealthCamera(float healthCameraIntensity) => OnChangeHealthCamera?.Invoke(healthCameraIntensity);
    public static event Action<float> OnChangeHealthBar;
    public static void ChangeHealthBar(float newHealth) => OnChangeHealthBar?.Invoke(newHealth);
    public static event Action<GameObject, float> OnChangeHealthForEntity;
    public static void ChangeHealthForEntity(GameObject entity, float newHealth) => OnChangeHealthForEntity?.Invoke(entity, newHealth);

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

}
