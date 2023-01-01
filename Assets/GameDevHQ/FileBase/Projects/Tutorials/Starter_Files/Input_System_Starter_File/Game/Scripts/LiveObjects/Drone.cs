using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }
        InputActionMaps _input;
        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private void Awake()
        {
            _input = new InputActionMaps();
        }

        private void OnEnable()
        { 
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
            _input.Drone.Exit.performed += Exit_performed;
        }

        private void Exit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ExitFlightMode();
            _input.Player.Enable();
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _input.Player.Disable();
                _input.Drone.Enable();
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            onExitFlightmode?.Invoke();
            UIManager.Instance.DroneView(false);            
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
           var rotate = _input.Drone.Rotate.ReadValue<float>();
            if (rotate == -1)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y -= _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            else if (rotate == 1)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
        }

        private void CalculateMovementFixedUpdate()
        {
            var  movement = _input.Drone.Flight.ReadValue<Vector3>();
            _rigidbody.AddForce( new Vector3(0,movement.y,0) * _speed, ForceMode.Acceleration);
        }


        private void CalculateTilt()
        {
            var movement = _input.Drone.Flight.ReadValue<Vector3>();
            Debug.Log(_input.Drone.Flight.inProgress);
            if(_input.Drone.Flight.inProgress)
            transform.rotation = Quaternion.Euler(movement.z * 30, transform.localRotation.eulerAngles.y, movement.x * -30);
            else{ transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0); }
            
            
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }
    }
}
