using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MuchroomAnimator
{
    public static class Params
    {
        public const string IsRunning = "IsRunning";
    }

    public static class States
    {
        public const string Idle_Mushroom = nameof(Idle_Mushroom);
        public const string Run_Mushroom = nameof(Run_Mushroom);
    }
}
public class Enemy : MonoBehaviour
{
    [SerializeField] private Transform _path;
    [SerializeField] private float _speed;

    private Animator _animator;

    private Transform[] _points;

    private int _currentPoint;

    private float _minDistance = 0.5f;
    private float _timeWaiting = 2f;
    private float _waitTimer = 0;

    private bool _isFaceRight = false;

    private void Start()
    {
        _animator= GetComponent<Animator>();
        _points= new Transform[_path.childCount];
        for(int i = 0; i< _points.Length; i++)
        {
            _points[i] = _path.GetChild(i);
        }
    }

    private void Update()
    {
        Transform target = _points[_currentPoint];

        if (Vector2.Distance(transform.position, target.position) < _minDistance)
        {
            _animator.SetBool(MuchroomAnimator.Params.IsRunning, false);

            if (_waitTimer >= _timeWaiting)
            {
                _currentPoint++;
                _waitTimer = 0;
                _isFaceRight = !_isFaceRight; 
            }
            else
            {
                _waitTimer += Time.deltaTime;
            }

            if (_currentPoint >= _points.Length)
            {
                _currentPoint = 0;
            }
        }
        else
        {
            Reflect();
            transform.position = Vector2.MoveTowards(transform.position, target.position, _speed * Time.deltaTime);
            _animator.SetBool(MuchroomAnimator.Params.IsRunning, true);
        }
    }

    private void Reflect()
    {
        if (_isFaceRight)
        {
            transform.localScale = new Vector2 (1,1);
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
        }
    }
}
