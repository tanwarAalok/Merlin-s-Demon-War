using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Effect : MonoBehaviour
{
    public Player targetPlayer = null;
    public Card sourceCard = null;
    public Image effectImage = null;
    public AudioSource iceAudio = null;
    public AudioSource fireAudio = null;
    public AudioSource destructAudio = null;

    public void EndTrigger()
    {
        bool bounce = false;
        if (targetPlayer.hasMirror())
        {
            bounce = true;
            targetPlayer.SetMirror(false);
            targetPlayer.PlaySmashSound();
            if (targetPlayer.isPlayer)
            {
                GameController.instance.CastAttackEffect(sourceCard, GameController.instance.enemy);
            }
            else 
            {
                GameController.instance.CastAttackEffect(sourceCard, GameController.instance.player);
            }
        }
        else 
        {
            int damage = sourceCard.cardData.damage;
            if(!targetPlayer.isPlayer)  // enemy
            {
                if(sourceCard.cardData.damageType == CardData.DamageType.Fire && targetPlayer.isFire)
                {
                    damage = damage / 2;
                }
                if(sourceCard.cardData.damageType == CardData.DamageType.Ice && !targetPlayer.isFire)
                {
                    damage = damage / 2;
                }
            }
            targetPlayer.health -= damage;
            targetPlayer.PlayHitAnim();

            GameController.instance.UpdateHealths();

            if(targetPlayer.health <= 0)
            {
                targetPlayer.health = 0;
                if(targetPlayer.isPlayer)
                {
                    GameController.instance.PlayPlayerDieSound();
                }
                else
                {
                    GameController.instance.PlayEnemyDieSound();
                }
            }

            if (!bounce)
            {
                if(targetPlayer.isPlayer) {
                    Debug.Log("merlins turn now changin");
                }else Debug.Log("enemys turn now changin");

                GameController.instance.NextPlayersTurn(); 
            }
            GameController.instance.isPlayable = true;
            
        }
        
        Destroy(gameObject);
    }

    internal void PlayIceAudio()
    {
        iceAudio.Play();
    }
    internal void PlayFireAudio()
    {
        fireAudio.Play();
    }
    internal void PlayBoomAudio()
    {
        destructAudio.Play();
    }
}
