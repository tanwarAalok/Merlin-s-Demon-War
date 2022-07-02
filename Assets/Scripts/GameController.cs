using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    static public GameController instance = null;
    public Deck enemyDeck = new Deck();
    public Deck playerDeck = new Deck();
    public Hand playersHand = new Hand();
    public Hand enemysHand = new Hand();
    public List<CardData> cards = new List<CardData>();
    public Sprite[] healthNumbers = new Sprite[10];
    public Sprite[] damageNumbers = new Sprite[10];
    public GameObject cardPrefab = null;
    public Canvas canvas = null;
    public bool isPlayable = false;

    public Player player = null;
    public Player enemy = null;

    public GameObject effectFromLeftPrefab = null;
    public GameObject effectFromRightPrefab = null;
    public Sprite fireBallImage = null;
    public Sprite iceBallImage = null;
    public Sprite mulitfireBallImage = null;
    public Sprite mulitIceBallImage = null;
    public Sprite fireAndIceBallImage = null;
    public Sprite destroyBallImage = null;
    public bool playersTurn = true;
    public TMP_Text turnText = null;
    public Image enemySkipTurn = null;
    public Sprite fireDemon = null;
    public Sprite iceDemon = null;
    public TMP_Text scoreText = null;

    public int playerScore = 0;
    public int playerKills = 0;
    public AudioSource playerDieAudio = null;
    public AudioSource enemyDieAudio = null;

    private void Awake() {
        instance = this;

        SetUpEnemy();

        playerDeck.Create();
        enemyDeck.Create();

        StartCoroutine(Dealhands());
        
    }
    public void Quit(){
        SceneManager.LoadScene(0);
    }

    public void SkipTurn(){
        if(playersTurn && isPlayable) NextPlayersTurn();
    }

    internal IEnumerator Dealhands()
    {
        yield return new WaitForSeconds(1);
        for (int t = 0; t < 3; t++){
            playerDeck.DealCard(playersHand);
            enemyDeck.DealCard(enemysHand);
            yield return new WaitForSeconds(1);
        }
        isPlayable = true;
    }

    internal bool UseCard(Card card, Player usingOnPlayer, Hand fromHand)
    {
        // DealReplacementCard
        if(!CardValid(card, usingOnPlayer, fromHand)) {
            Debug.Log("Card not valid");
            return false;
        };

        isPlayable = false;

        CastCard(card, usingOnPlayer, fromHand);

        player.glowImage.gameObject.SetActive(false);
        enemy.glowImage.gameObject.SetActive(false);

        fromHand.RemoveCard(card);

        return false;
    }

    internal bool CardValid(Card cardBeingPlayed, Player usingOnPlayer, Hand fromHand)
    {
        bool valid = false;

        if(cardBeingPlayed == null)
        {
            Debug.Log("Card played is null");
            return false;
        }

        if(fromHand.isPlayers){
            if(cardBeingPlayed.cardData.cost <= player.mana)
            {
                if(usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenceCard)
                {
                    valid = true;
                }
                if(!usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenceCard)
                {
                    valid = true;
                }
            }
            else Debug.Log("cost is greater than mana for player");
        }
        else{
            if(cardBeingPlayed.cardData.cost <= enemy.mana)
            {
                if(!usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenceCard)
                {
                    valid = true;
                }
                if(usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenceCard)
                {
                    valid = true;
                }
            }
            else Debug.Log("cost is greater than mana for enemy");

        }
        return valid;
    }

    internal void CastCard(Card card, Player usingOnPlayer, Hand fromHand)
    {
        if(card.cardData.isMirrorCard)
        {
            usingOnPlayer.SetMirror(true);
            usingOnPlayer.PlayMirrorSound();
            NextPlayersTurn();
            isPlayable = true;
        }
        else
        {
            if(card.cardData.isDefenceCard)
            {
                usingOnPlayer.health += card.cardData.damage;
                usingOnPlayer.PlayHealSound();
                if(usingOnPlayer.health > usingOnPlayer.maxHealth) usingOnPlayer.health = usingOnPlayer.maxHealth;

                UpdateHealths();
                StartCoroutine(CastHealEffect(usingOnPlayer));
            }
            else // attack card
            {
                CastAttackEffect(card, usingOnPlayer);
            }
        
            if(fromHand.isPlayers)
            {
                playerScore += card.cardData.damage;
            }

            UpdateScore();
        }
        
        if(fromHand.isPlayers)
        {
            GameController.instance.player.mana -= card.cardData.cost;
            GameController.instance.player.UpdateManaBalls();
        }
        else {
            GameController.instance.enemy.mana -= card.cardData.cost;
            GameController.instance.enemy.UpdateManaBalls();
        }
    }

    private IEnumerator CastHealEffect(Player usingOnPlayer)
    {
        yield return new WaitForSeconds(0.5f);
        NextPlayersTurn();
        isPlayable = true;
    }

    internal void CastAttackEffect(Card card, Player usingOnPlayer)
    {
        GameObject effectGO = null;
        if(usingOnPlayer.isPlayer)
        {
            effectGO = Instantiate(effectFromRightPrefab, canvas.gameObject.transform);

        }
        else {
            effectGO = Instantiate(effectFromLeftPrefab, canvas.gameObject.transform);
        }

        Effect effect = effectGO.GetComponent<Effect>();
     
        if(effect)
        {
            effect.targetPlayer = usingOnPlayer;
            effect.sourceCard = card;

            switch(card.cardData.damageType)
            {
                case CardData.DamageType.Fire:
                    if(card.cardData.isMulti)
                    {
                        effect.effectImage.sprite = mulitfireBallImage;
                    }
                    else {
                        effect.effectImage.sprite = fireBallImage;
                    }
                    effect.PlayFireAudio();
                break;

                case CardData.DamageType.Ice:
                    if(card.cardData.isMulti)
                    {
                        effect.effectImage.sprite = mulitIceBallImage;
                    }
                    else {
                        effect.effectImage.sprite = iceBallImage;
                    }
                    effect.PlayIceAudio();
                break;

                case CardData.DamageType.Both:
                    effect.effectImage.sprite = fireAndIceBallImage;
                    effect.PlayFireAudio();
                    effect.PlayIceAudio();
                break;

                case CardData.DamageType.Destruct:
                    effect.effectImage.sprite = destroyBallImage;
                    effect.PlayBoomAudio();
                break;
            }
        }
        else{
            Debug.Log("effect not found");
        }
    }

    internal void UpdateHealths()
    {
        player.UpdateHealth();
        enemy.UpdateHealth();

        if(player.health <= 0)
        {
            StartCoroutine(GameOver());
        }

        if(enemy.health <=0)
        {
            playerKills++;
            playerScore += 100;
            UpdateScore();
            StartCoroutine(NewEnemy());
        }
    }
    private IEnumerator NewEnemy()
    {
        enemy.gameObject.SetActive(false);
        enemysHand.ClearHand();
        yield return new WaitForSeconds(0.75f);
        SetUpEnemy();
        enemy.gameObject.SetActive(true);
        StartCoroutine(Dealhands());
    }
    private void SetUpEnemy()
    {
        enemy.mana = 0;
        enemy.health = 5;
        enemy.UpdateHealth();
        enemy.isFire = true;
        if(UnityEngine.Random.Range(0, 2) == 1)
        {
            enemy.isFire = false;
        }
        if(enemy.isFire)
        {
            enemy.playerImage.sprite = fireDemon;
        }
        else enemy.playerImage.sprite = iceDemon;
    }
    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    internal void NextPlayersTurn()
    {
        playersTurn = !playersTurn;
        bool enemyIsDead = false;

        if(playersTurn)
        {
            if(player.mana < 5) {
                player.mana++;
                player.UpdateManaBalls();
            }
        }
        else
        {
            if(enemy.health > 0){
                if(enemy.mana < 5) {
                    enemy.mana++;
                    enemy.UpdateManaBalls();
                }
            }
            else enemyIsDead = true;
        }

        if(enemyIsDead)
        {
            playersTurn = !playersTurn;
            if (player.mana < 5)
            {
                player.mana++;
                player.UpdateManaBalls();
            }
        }
        else{
            SetTurnText();
            if(!playersTurn) EnemysTurn();
        }

        player.UpdateManaBalls();
        enemy.UpdateManaBalls();
    }

    internal void SetTurnText()
    {
        if(playersTurn) 
        {
            turnText.text = "Merlin's Turn";
        }
        else 
        {
            turnText.text = "Enemy's Turn";
        }
    }

    private void EnemysTurn()
    {
        Card card = AIChooseCard();
        StartCoroutine(EnemyCastCard(card));
    }

    private Card AIChooseCard()
    {
        List<Card> available = new List<Card>();
        for (int i = 0; i < 3; i++)
        {
            if(CardValid(enemysHand.cards[i], enemy, enemysHand))
            {
                available.Add(enemysHand.cards[i]);
            }
            else if(CardValid(enemysHand.cards[i], player, enemysHand))
            {
                available.Add(enemysHand.cards[i]);
            }
        }

        if(available.Count == 0) 
        {
            NextPlayersTurn();
            return null;
        }
        int choice = UnityEngine.Random.Range(0, available.Count);
        return available[choice];
    }

    private IEnumerator EnemyCastCard(Card card)
    {
        yield return new WaitForSeconds(0.5f);

        if(card)
        {
            TurnCard(card);
            yield return new WaitForSeconds(2);

            if(card.cardData.isDefenceCard)
            {
                UseCard(card, enemy, enemysHand);
            }
            else UseCard(card, player, enemysHand);
            yield return new WaitForSeconds(1);

            enemyDeck.DealCard(enemysHand);
            yield return new WaitForSeconds(1);
        }
        else // no card to choose, so skip turn
        {
            enemySkipTurn.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
            enemySkipTurn.gameObject.SetActive(false);
        }
    }

    internal void TurnCard(Card card)
    {
        Animator animator = card.GetComponentInChildren<Animator>();
        if(animator)
        {
            animator.SetTrigger("Flip");
        }
        else Debug.Log("No animator found");
    }

    private void UpdateScore()
    {
        scoreText.text = "Demons Killed: " + playerKills.ToString()+". Score: "+ playerScore.ToString();
    }

    internal void PlayPlayerDieSound()
    {
        playerDieAudio.Play();
    }
    internal void PlayEnemyDieSound()
    {
        enemyDieAudio.Play();
    }
}
