using System;
using UnityEngine;
using Cinemachine;

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;
        private InputActionMaps _input;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

        private void Awake()
        {
            _input = new InputActionMaps();
        }
        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
            _input.Forklift.Exit.performed += Exit_performed;
        }

        private void Exit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ExitDriveMode();
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _input.Player.Disable();
                _input.Forklift.Enable();
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);
            }
        }

        private void ExitDriveMode()
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;            
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
            
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                LiftControls();
                CalcutateMovement();
            }

        }

        private void CalcutateMovement()
        {
           var movement = _input.Forklift.Drive.ReadValue<Vector2>();
           var direction = new Vector3(0, 0, movement.y);
           var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(movement.y) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += (movement.x * - 1) * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        private void LiftControls()
        {
            var LiftUpOrDown = _input.Forklift.Lift.ReadValue<float>();
                LiftUpRoutine(LiftUpOrDown);
        }

        private void LiftUpRoutine(float LiftUpOrDown)
        {
            if (LiftUpOrDown == 1)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
                if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                    _lift.transform.localPosition = _liftUpperLimit;
            }
                           
            if (LiftUpOrDown == -1)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
                if(_lift.transform.localPosition.y <= _liftLowerLimit.y)
                    _lift.transform.localPosition = _liftLowerLimit;
            }                
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
            _input.Forklift.Disable();
        }

    }
}