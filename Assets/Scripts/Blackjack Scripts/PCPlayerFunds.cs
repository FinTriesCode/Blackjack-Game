using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;  // Import Unity Events namespace

public class PCPlayerFunds : MonoBehaviour
{
    public float currentBalance = 300;
    private float tempBalance = 0;

    public UnityEvent<float> OnBalanceChanged;  // Event to notify when the balance changes

    void Awake()
    {
        if (OnBalanceChanged == null)
            OnBalanceChanged = new UnityEvent<float>();
    }

    public void IncreaseBalance(float addBalanceAmount)
    {
        currentBalance += addBalanceAmount;
        OnBalanceChanged.Invoke(currentBalance);  // Invoke the event with the updated balance
    }

    public void DecreaseBalance(float removeBalanceAmount)
    {
        if (currentBalance - removeBalanceAmount >= 0)  // Check to prevent negative balance
        {
            currentBalance -= removeBalanceAmount;
            OnBalanceChanged.Invoke(currentBalance);
        }
        else
        {
            Debug.Log("Not enough funds to decrease.");
        }
    }

    public void TempBalance(float amountDrawn)
    {
        if (currentBalance - amountDrawn >= 0)  // Ensure enough funds are available
        {
            tempBalance += amountDrawn;
            currentBalance -= amountDrawn;
            OnBalanceChanged.Invoke(currentBalance);
        }
        else
        {
            Debug.Log("Not enough funds to reserve.");
        }
    }
}