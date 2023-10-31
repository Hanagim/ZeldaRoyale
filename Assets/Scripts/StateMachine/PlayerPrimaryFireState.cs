using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryFireState : PlayerBaseState
{
    IEnumerator IAttackResetRoutine()
    {
        yield return new WaitForSeconds(.5f);
        Ctx.JumpCount = 0;
    }
    public PlayerPrimaryFireState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

  
    public override void EnterState()
    {
        Debug.Log("Entered primary fire state");
        Ctx.Animator.SetBool(Ctx.IsSwordAndShieldAttackingHash, true);
        Ctx.AttackComboCount++;
        Ctx.Animator.SetInteger(Ctx.AttackComboCountHash, Ctx.AttackComboCount);
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(Ctx.IsSwordAndShieldAttackingHash, false);
        if (Ctx.IsPrimaryFirePressed)
        {
            Ctx.RequireNewPrimaryFirePress = true;
        }

        Ctx.CurrentComboAttackResetRoutine = Ctx.StartCoroutine(IAttackResetRoutine());
        if (Ctx.AttackComboCount == 3)
        {
            Ctx.JumpCount = 0;
            Ctx.Animator.SetInteger(Ctx.AttackComboCountHash, Ctx.AttackComboCount);
        }
        Debug.Log("Exited state");
    }

    public override void InitializeSubState()
    {

    }

    public override void CheckSwitchStates()
    {
        
    }
}
