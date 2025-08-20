using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
public class MohsTest : MonoBehaviour
{
    [SerializeField] List<Progress> progreses;
    [System.Serializable]
    public struct Progress
    {
        public LineRenderer lr;
        public Coroutine co;
    }
    public bool Has(LineRenderer lr)
    {
        int find = progreses.FindIndex(x => x.lr == lr);
        if (find == -1)
            return false;
        else
            return true;
    }
    public void AddScratch(LineRenderer lr)
    {
        Progress pg = new Progress();
        pg.lr = lr;
        pg.co = StartCoroutine(AutoRemove(lr));
        progreses.Add(pg);
    }
    public void ReCoroutine(LineRenderer lr)
    {
        int find = progreses.FindIndex(x => x.lr == lr);
        if (find != -1)
        {
            Progress pg = progreses[find];
            StopCoroutine(pg.co);
            pg.co = StartCoroutine(AutoRemove(lr));
            progreses[find] = pg;
        }
    }
    IEnumerator AutoRemove(LineRenderer lr)
    {
        float time = Time.time;
        YieldInstruction yi = new WaitForSeconds(1f);
        while (true)
        {
            yield return yi;
            if (Time.time - time < 20f) continue;
            int find = progreses.FindIndex(x => x.lr == lr);
            if (find != -1)
                progreses.RemoveAt(find);
            break;
        }
        Debug.Log($"시간이 오래지나서 {lr}의 긁힌자국 제거");
    }










}
