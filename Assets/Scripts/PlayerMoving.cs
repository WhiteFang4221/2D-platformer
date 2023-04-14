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
        public const string IsOnWall = "IsOnWall";

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
    private float _groundCheckRadius;
    private bool _isOnGround;

    [SerializeField] private Transform _wallCheckerUp;
    [SerializeField] private Transform _wallCheckerDown;
    [SerializeField] private LayerMask _wall;
    [SerializeField] private float _slideSpeed = 4f;
    private float _wallCheckRadius;
    private bool _isOnWall;


    private Rigidbody2D _rigidbody;
    private Animator _animator;

    private Vector2 _moveVector;

    private bool _isfaceRight = true;


    private bool _isCanRoll = true;
    private bool _isRolling;
    private float _rollPower = 15f;
    private float _rollingTime = 0.5f;
    private float _rollCooldown = 1f;

    private float _gravityDefault;



    private void Start()
    {
        _groundCheckRadius = _groundChecker.GetComponent<CircleCollider2D>().radius;
        _wallCheckRadius = _wallCheckerUp.GetComponent<CircleCollider2D>().radius;
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator= GetComponent<Animator>();
        _gravityDefault = _rigidbody.gravityScale;
    }

    private void Update()
    {
        if (!_isRolling)
        {
        Walk();
        }

        Reflect();
        Jump();
        CheckGround();
        CheckWall();

        MoveOnWall();

        
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && _isOnGround && _isCanRoll)
        {
            _isCanRoll = false;
            _isRolling = true;
            _rigidbody.velocity = new Vector2(0, 0);
            _animator.SetTrigger(AnimatorPlayerController.Params.RollTrigger);

            if (_isfaceRight)
            {
                _rigidbody.AddForce(new Vector2(_rollPower * 1, 0f), ForceMode2D.Impulse);
            }
            else
            {
                _rigidbody.AddForce(new Vector2(_rollPower * (-1), 0f), ForceMode2D.Impulse);
            }

            float gravity = _rigidbody.gravityScale;
            _rigidbody.gravityScale = 0;
            _isRolling = false;
            _rigidbody.gravityScale = gravity;
            _isCanRoll = true;
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
        if(Input.GetKeyDown(KeyCode.Space) && _isOnGround == true && _isRolling == false)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _jumpForce);
        }
    }

    private IEnumerator Roll()
    {
        _isCanRoll= false;
        _isRolling = true;
        _rigidbody.velocity = new Vector2(0,0);
        _animator.SetTrigger(AnimatorPlayerController.Params.RollTrigger);

        if (_isfaceRight)
        {
            _rigidbody.AddForce(new Vector2(_rollPower * 1, 0f), ForceMode2D.Impulse);
        }
        else
        {
            _rigidbody.AddForce(new Vector2(_rollPower * (-1), 0f), ForceMode2D.Impulse);
        }

        float gravity = _rigidbody.gravityScale;
        _rigidbody.gravityScale = 0;
        yield return new WaitForSeconds(_rollingTime);
        _isRolling = false;
        _rigidbody.gravityScale = gravity;
        yield return new WaitForSeconds(_rollCooldown);
        _isCanRoll= true;
    }

    private void CheckGround()
    {
        _isOnGround = Physics2D.OverlapCircle(_groundChecker.position, _groundCheckRadius, _groundLayer);
        _animator.SetBool (AnimatorPlayerController.Params.IsOnGround, _isOnGround);
        _animator.SetFloat(AnimatorPlayerController.Params.AirSpeedY, _rigidbody.velocity.y);
    }
    
    private void CheckWall()
    {
        _isOnWall = Physics2D.OverlapCircle(_wallCheckerUp.position, _wallCheckRadius, _wall) && Physics2D.OverlapCircle(_wallCheckerDown.position, _wallCheckRadius, _wall);
    }

    private void MoveOnWall()
    {
        if (_isOnWall && !_isOnGround) 
        {
            _rigidbody.gravityScale = 0;
            _rigidbody.velocity = new Vector2(0, -_slideSpeed);
            _animator.SetBool(AnimatorPlayerController.Params.IsOnWall, true);
        }
        else if (!_isOnWall || _isOnGround)
        {
            _rigidbody.gravityScale = _gravityDefault;
            _animator.SetBool(AnimatorPlayerController.Params.IsOnWall, false);
        }
    }
}
