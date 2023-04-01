using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]

public static class AnimatorPlayerController
{
    public static class Params
    {
        public const string MoveX = "MoveX";
        public const string AttackTrigger = "AttackTrigger";
        public const string IsOnGround = "IsOnGround";
        public const string AirSpeedY = "AirSpeedY";
        public const string RollTrigger = "RollTrigger";

        public static class States
        {
            public const string Idle = nameof(Idle);
            public const string Run = nameof(Run);
            public const string Attack1 = nameof(Attack1);
            public const string Jump = nameof(Jump);
            public const string Fall = nameof(Fall);
            public const string Roll = nameof(Roll);
        }
    }
}

public class PlayerMoving : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;

    [SerializeField] private float _jumpForce = 5f;

    [SerializeField] private Transform _groundChecker;
    [SerializeField] private LayerMask _groundLayer;
    private float _checkRadius = 0.5f;

    private Rigidbody2D _rigidbody;
    private Animator _animator;

    private Vector2 _moveVector;

    private bool _isfaceRight = true;

    private bool _isOnGround;

    private float _rollForce = 100f;

    private void Start()
    {
        _rigidbody= GetComponent<Rigidbody2D>();
        _animator= GetComponent<Animator>();
    }

    private void Update()
    {
        
        Walk();
        Reflect();
        Jump();
        CheckGround();

        if (Input.GetKeyDown(KeyCode.LeftShift) && _isOnGround == true)
        {
            StartCoroutine(Roll());
        }


    }

    private void Walk()
    {
        _moveVector.x = Input.GetAxis("Horizontal");
        _animator.SetFloat(AnimatorPlayerController.Params.MoveX, Mathf.Abs(_moveVector.x));
        _rigidbody.velocity = new Vector2(_moveVector.x * _speed, _rigidbody.velocity.y);
    }

    private void Reflect()
    {
        if ((_moveVector.x > 0 && !_isfaceRight || (_moveVector.x < 0 && _isfaceRight)))
        {
            transform.localScale *= new Vector2(-1, 1);
            _isfaceRight= !_isfaceRight;
        }
    }

    private void Jump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && _isOnGround == true)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _jumpForce);
        }
    }

    private IEnumerator Roll()
    {

            _animator.SetTrigger(AnimatorPlayerController.Params.RollTrigger);
            _speed *= 3;
        yield return new WaitForSeconds(0.5f);
        _speed = 5f;
    }

    private void CheckGround()
    {
        _isOnGround = Physics2D.OverlapCircle(_groundChecker.position, _checkRadius, _groundLayer);
        _animator.SetBool (AnimatorPlayerController.Params.IsOnGround, _isOnGround);
        _animator.SetFloat(AnimatorPlayerController.Params.AirSpeedY, _rigidbody.velocity.y);
    }
}
