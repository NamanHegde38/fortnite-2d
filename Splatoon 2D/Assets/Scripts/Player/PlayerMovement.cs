﻿using UnityEngine;

namespace Player {
	[RequireComponent (typeof (PlayerController))]
	[RequireComponent (typeof (PlayerManager))]
	[RequireComponent (typeof (PlayerSquid))]
	public class PlayerMovement : MonoBehaviour {

		public float moveSpeed = 6f;
		public float slowedMoveSpeed = 3f;

		public float squidMoveSpeed = 2f;
		public float squidInkSpeed = 10f;
		public float squidSlowedSpeed = 1f;
		
		private float _currentSpeed;
	
		public float jumpHeight = 4f;
		[Range (.1f, 1f)] public float timeToJumpApex = .4f;

		private const float AccelerationTimeAir = .1f;
		public float accelerationTimeGround = .05f;
		public float accelerationTimeSquid = .25f;

		private float _currentAccelerationTime;

		private float _gravity;

		private float _jumpVelocity;

		private float _jumpCooldown = .1f;
		private bool _ableToJump;

		private bool _hasSetCooldown;

		private Vector3 _velocity;
		private float _velocityXSmoothing;
		private Vector2 _directionalInput;

	

		public bool facingRight = true;

		private bool _isSquid;
		
		private PlayerController _controller;
		private PlayerManager _manager;
		private PlayerSquid _squid;

		private void Start () {
			SetComponents();
			SetPhysics();
		}

		private void SetPhysics() {
			_gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
			_jumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
		}

		private void SetComponents() {
			_controller = GetComponent<PlayerController>();
			_manager = GetComponent<PlayerManager>();
			_squid = GetComponent<PlayerSquid>();
		}

		public void SetDirectionalInput (Vector2 input) {
			_directionalInput = input;
		}

		public void OnJumpInputDown () {
			if (_ableToJump) {
				if (_isSquid) {
					jumpHeight = 1.25f;
					timeToJumpApex = .3f;
					SetPhysics();
				}
				else {
					jumpHeight = 2.5f;
					timeToJumpApex = .4f;
					SetPhysics();
				}
				_velocity.y = _jumpVelocity;
				_ableToJump = false;
			}
		}

		private void Update () {
			SetMovementSpeed();
			SetVelocity();
			SetJumpDelay();
			CheckIfSquid();
			
		}

		private void SetMovementSpeed() {
			var groundInk = _manager.groundInk;
			if (_isSquid) {
				if (groundInk == _manager.unpaintedGround) {
					_currentSpeed = squidMoveSpeed;
				}
				else if (groundInk == _manager.teamPaintedGround) {
					_currentSpeed = squidInkSpeed;
				}
				else if (groundInk == _manager.enemyPaintedGround) {
					_currentSpeed = squidSlowedSpeed;
				}

				_currentAccelerationTime = accelerationTimeSquid;
			}

			else if (!_isSquid){
				if (groundInk == _manager.unpaintedGround) {
					_currentSpeed = moveSpeed;
				}
				
				else if (groundInk == _manager.teamPaintedGround) {
					_currentSpeed = moveSpeed;
				}
				
				else if (groundInk == _manager.enemyPaintedGround) {
					_currentSpeed = slowedMoveSpeed;
				}

				_currentAccelerationTime = accelerationTimeGround;
			}
		}
		
		private void SetVelocity() {
			var targetVelocityX = _directionalInput.x * _currentSpeed;

			_velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing,
				(_controller.collisions.Below) ? _currentAccelerationTime : AccelerationTimeAir);

			_velocity.y += _gravity * Time.deltaTime;

			_controller.Move(_velocity * Time.deltaTime);

			if (_controller.collisions.Above || _controller.collisions.Below) {
				_velocity.y = 0;
			}
		}

		private void SetJumpDelay() {
			if (!_controller.collisions.Below) {
				if (!_hasSetCooldown) {
					_jumpCooldown = 0.1f;
					_hasSetCooldown = true;
				}
			}

			else {
				_hasSetCooldown = false;
				_ableToJump = true;
			}

			if (_jumpCooldown <= 0 && !_controller.collisions.Below) {
				_ableToJump = false;
			}

			else {
				_jumpCooldown -= Time.deltaTime;
				_ableToJump = true;
			}
		}

		private void CheckIfSquid() {
			_isSquid = _squid.isSquid;
		}
		
		public void Flip () {
			facingRight = !facingRight;

			var transformVariable = transform;
			var scale = transformVariable.localScale;
			scale.x *= -1;

			transformVariable.localScale = scale;
		}
	}
}