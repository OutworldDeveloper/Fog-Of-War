using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Hunter : MonoBehaviour
{

    [SerializeField] private Game _game;
    [SerializeField] private LayerMask _layerMask;

    private NavMeshAgent _agent;
    private AIState _aiState;
    private Vector3 _lastHeardNoisePosition;
    private bool _isPlayerVisible;
    private Vector3 _lastPlayerSeenPosition;
    private float _lastPlayerSeenTime;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        var playerDirection = _game.PlayerCharacter.position + Vector3.up - transform.position + Vector3.up;
        playerDirection.Normalize();

        var ray = new Ray(transform.position + Vector3.up, playerDirection);

        _isPlayerVisible = Physics.Raycast(ray, out var hit, 15f, _layerMask) == false;

        if (_isPlayerVisible == true)
        {
            _lastPlayerSeenPosition = _game.PlayerCharacter.position;
            _lastPlayerSeenTime = Time.time;
        }

        if (_lastPlayerSeenTime + 1f < Time.time)
        {
            _lastPlayerSeenPosition = _game.PlayerCharacter.position;
        }

        //if (_isPlayerVisible == true)
        //    Debug.DrawLine(transform.position + Vector3.up, _game.PlayerCharacter.position + Vector3.up, Color.green);
        //else
        //    Debug.DrawLine(transform.position + Vector3.up, _game.PlayerCharacter.position + Vector3.up, Color.red);

        if (_aiState == AIState.Attack)
        {
            if (_isPlayerVisible == false)
            {
                ChangeAIState(AIState.CheckLastPosition);
            }
            else
            {
                _agent.SetDestination(_game.PlayerCharacter.position);
            }
        }
        else
        {
            if (_isPlayerVisible == true)
            {
                ChangeAIState(AIState.Attack);
            }
        }
    }

    private void ChangeAIState(AIState newState)
    {
        _aiState = newState;

        switch (_aiState)
        {
            case AIState.None:
                _agent.ResetPath();
                break;
            case AIState.CheckNoise:
                _agent.SetDestination(_lastHeardNoisePosition);
                break;
            case AIState.CheckLastPosition:
                _agent.SetDestination(_lastPlayerSeenPosition);
                break;
            case AIState.Attack:
                break;
            default:
                break;
        }
    }

    public void OnNoiseHeard(Vector3 position)
    {
        _lastHeardNoisePosition = position;

        if (_aiState != AIState.Attack)
        {
            ChangeAIState(AIState.CheckNoise);
        }
    }

    private enum AIState
    {
        None,
        CheckNoise,
        CheckLastPosition,
        Attack,
    }

}