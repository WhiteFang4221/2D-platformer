using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]

public static class PlayerAnimator
{
    public static class Params
    {
        public const string MoveX = "MoveX";
        public const string IsOnGround = "IsOnGround";
        public const string AirSpeedY = "AirSpeedY";
        public const string RollTrigger = "RollTrigger";
        public const string IsOnWall = "IsOnWall";
        public const string IsRolling = "IsRolling";
        public const string JumpTrigger = "JumpTrigger";
        public const string DamageTrigger = "DamageTrigger";

        public static class States
        {
            public const string Idle = nameof(Idle);
            public const string Run = nameof(Run);
            public const string Jump = nameof(Jump);
            public const string Fall = nameof(Fall);
            public const string Roll = nameof(Roll);
            public const string Hurt = nameof(Hurt);
        }
    }
}

public class PlayerMoving : MonoBehaviour
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _jumpForce = 20f;
    [SerializeField] private Transform _groundChecker;

    [SerializeField] private Transform _wallCheckerUp;
    [SerializeField] private Transform _wallCheckerDown;

    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wall;

    [SerializeField] private float _slideSpeed = 4f;

    private Vector2 _moveVector;
    private Vector2 _jumpAngle = new Vector2(10f, 17f);

    private Rigidbody2D _rigidbody;
    private Animator _animator;

    private float _gravityDefault;
    private float _groundCheckRadius;
    private float _wallCheckRadius;

    private float _rollPower = 15;
    private float _rollingTime = 0.5f;
    private float _rollCooldown = 0.5f;

    private float _wallJumpTime = 0.3f;
    private float _timerWallJump;

    private bool _isfaceRight = true;

    private bool _isOnGround;
    private bool _isOnWall;

    private bool _isCanRoll = true;
    private bool _isRolling;

    private bool _isWallJumping;



    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _groundCheckRadius = _groundChecker.GetComponent<CircleCollider2D>().radius;
        _wallCheckRadius = _wallCheckerUp.GetComponent<CircleCollider2D>().radius;
        _gravityDefault = _rigidbody.gravityScale;
    }
    
    private void Update()
    {
        if (!_isRolling && !_isWallJumping)
        {
        Walk();
        }

        Reflect();

        if (Input.GetKeyDown(KeyCode.Space) && _isOnGround == true && _isRolling == false)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && _isCanRoll && _isOnGround)
        {
            StartCoroutine(Roll());
        }

        WallJump();
        CheckGround();
        CheckWall();
        MoveOnWall();
    }

    private void Walk()
    {
        _moveVector.x = Input.GetAxis("Horizontal");
        _animator.SetFloat(PlayerAnimator.Params.MoveX, Mathf.Abs(_moveVector.x));
        _rigidbody.velocity = new Vector2(_moveVector.x * _speed, _rigidbody.velocity.y);
    }

    private void Reflect()
    {
        if ((_moveVector.x > 0 && !_isfaceRight || (_moveVector.x < 0 && _isfaceRight)))
        {
            transform.localScale *= new Vector2(-1, 1);
            _isfaceRight = !_isfaceRight;
        }
    }

    private void Jump()
    {     
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _jumpForce);
        _animator.SetTrigger(PlayerAnimator.Params.JumpTrigger);
    }

    private void CheckGround()
    {
        _isOnGround = Physics2D.OverlapCircle(_groundChecker.position, _groundCheckRadius, _groundLayer);
        _animator.SetBool(PlayerAnimator.Params.IsOnGround, _isOnGround);
        _animator.SetFloat(PlayerAnimator.Params.AirSpeedY, _rigidbody.velocity.y);

    }

    private void CheckWall()
    {
        _isOnWall = Physics2D.OverlapCircle(_wallCheckerUp.position, _wallCheckRadius, _wall) && Physics2D.OverlapCircle(_wallCheckerDown.position, _wallCheckRadius, _wall);
    }

    private void MoveOnWall()
    {
        if (_isOnWall && !_isOnGround && _rigidbody.velocity.y < 2)
        {
            _rigidbody.gravityScale = 0;
            _rigidbody.velocity = new Vector2(0, -_slideSpeed);
            _animator.SetBool(PlayerAnimator.Params.IsOnWall, true);
        }

        else if (!_isOnWall || _isOnGround)
        {
            _rigidbody.gravityScale = _gravityDefault;
            _animator.SetBool(PlayerAnimator.Params.IsOnWall, false);
        }
    }


    private IEnumerator Roll()
    {
        _isCanRoll = false;
        _isRolling = true;
        _rigidbody.velocity = new Vector2(0, 0);
        _animator.SetTrigger(PlayerAnimator.Params.RollTrigger);
        
        _animator.SetBool(PlayerAnimator.Params.IsRolling, _isRolling);

        if (_isfaceRight)
        {
            _rigidbody.AddForce(new Vector2(_rollPower * 1, 0f), ForceMode2D.Impulse);
        }
        else
        {
            _rigidbody.AddForce(new Vector2(_rollPower * (-1), 0f), ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(_rollingTime);
        _isRolling = false;
        _animator.SetBool(PlayerAnimator.Params.IsRolling, _isRolling);
        yield return new WaitForSeconds(_rollCooldown);
        _isCanRoll = true;
    }

    private void WallJump()
    {
        if (_isOnGround == false && _isOnWall && Input.GetKeyDown(KeyCode.Space) && _rigidbody.velocity.y < 0)
        {
            _isWallJumping = true;
            _moveVector.x = 0;
            transform.localScale *= new Vector2(-1, 1);
            _isfaceRight = !_isfaceRight;
            _rigidbody.gravityScale = _gravityDefault;
            _rigidbody.velocity = new Vector2(transform.localScale.x * _jumpAngle.x, _jumpAngle.y);
        }

        if (_isWallJumping && (_timerWallJump += Time.deltaTime) >= _wallJumpTime)
        {
            if (_isOnWall || _isOnGround || Input.GetAxisRaw("Horizontal") != 0)
            {
                _isWallJumping = false;
                _timerWallJump = 0;
            }
        }
    }
}
