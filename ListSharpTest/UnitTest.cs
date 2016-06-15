using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LispSharp;

namespace ListSharpTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void Basic()
        {
            Cell res = Program.Eval(Program.Parse("(quote (testing 1 (2.0) -3.14e9))"));
            Assert.AreEqual(res.Type, Cell.CellType.List);
            Assert.AreEqual(res.list.Count, 4);
            Assert.AreEqual(res.list[0].symbol, "testing");
            Assert.AreEqual(res.list[1].number, 1f);
            Assert.AreEqual(res.list[2].list[0].number, 2f);
            Assert.AreEqual(res.list[3].number, -3.14e9f);

            res = Program.Eval(Program.Parse("(+ 2 2)"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 4f);

            res = Program.Eval(Program.Parse("(+ (* 2 100) (* 1 10))"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 210f);

            res = Program.Eval(Program.Parse("(if (> 6 5) (+ 1 1) (+ 2 2))"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 2f);

            res = Program.Eval(Program.Parse("(if (< 6 5) (+ 1 1) (+ 2 2))"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 4f);
        }

        [TestMethod]
        public void Env()
        {

            Cell res = Program.Eval(Program.Parse("(define x 3)"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("x"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 3f);

            res = Program.Eval(Program.Parse("(+ x x)"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 6f);

            res = Program.Eval(Program.Parse("(begin (define x 1) (set! x (+ x 1)) (+ x 1))"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 3f);
        }

        [TestMethod]
        public void Lambda()
        {

            Cell res = Program.Eval(Program.Parse("((lambda (x) (+ x x)) 5)"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 10f);

            res = Program.Eval(Program.Parse("(define twice (lambda (x) (* 2 x)))"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("(twice 5)"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 10f);

            res = Program.Eval(Program.Parse("(define compose (lambda (f g) (lambda (x) (f (g x)))))"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("((compose list twice) 5)"));
            Assert.AreEqual(res.Type, Cell.CellType.List);
            Assert.AreEqual(res.list.Count, 1);
            Assert.AreEqual(res.list[0].number, 10f);

            res = Program.Eval(Program.Parse("(define repeat (lambda (f) (compose f f)))"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("((repeat twice) 5)"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 20f);

            res = Program.Eval(Program.Parse("((repeat (repeat twice)) 5)"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 80f);
        }

        [TestMethod]
        public void Recursion()
        {
            Cell res = Program.Eval(Program.Parse("(define fact (lambda (n) (if (<= n 1) 1 (* n (fact (- n 1))))))"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("(fact 3)"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 6f);

            res = Program.Eval(Program.Parse("(fact 10)"));
            Assert.AreEqual(res.Type, Cell.CellType.Number);
            Assert.AreEqual(res.number, 3628800f);
        }

        [TestMethod]
        public void List()
        {
            Cell res = Program.Eval(Program.Parse("(define abs (lambda (n) ((if (> n 0) + -) 0 n)))"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("(list (abs -3) (abs 0) (abs 3))"));
            Assert.AreEqual(res.Type, Cell.CellType.List);
            Assert.AreEqual(res.list.Count, 3);
            Assert.AreEqual(res.list[0].number, 3f);
            Assert.AreEqual(res.list[1].number, 0f);
            Assert.AreEqual(res.list[2].number, 3f);

            res = Program.Eval(Program.Parse(@"(define combine (lambda (f)
                                                (lambda (x y)
                                                    (if (null? x) (quote ())
                                                        (f (list (car x) (car y))
                                                            ((combine f) (cdr x) (cdr y)))))))"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("(define zip (combine cons))"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("(zip (list 1 2 3 4) (list 5 6 7 8))"));
            Assert.AreEqual(res.Type, Cell.CellType.List);
            Assert.AreEqual(res.list.Count, 4);
            Assert.AreEqual(res.list[0].list[0].number, 1f);
            Assert.AreEqual(res.list[0].list[1].number, 5f);
            Assert.AreEqual(res.list[1].list[0].number, 2f);
            Assert.AreEqual(res.list[1].list[1].number, 6f);
            Assert.AreEqual(res.list[2].list[0].number, 3f);
            Assert.AreEqual(res.list[2].list[1].number, 7f);
            Assert.AreEqual(res.list[3].list[0].number, 4f);
            Assert.AreEqual(res.list[3].list[1].number, 8f);

            res = Program.Eval(Program.Parse(@"
                (define riff-shuffle(lambda(deck)(begin
                    (define take(lambda(n seq)(if (<= n 0) (quote())(cons(car seq)(take(- n 1)(cdr seq))))))
                        (define drop(lambda(n seq)(if (<= n 0) seq(drop(- n 1)(cdr seq)))))
                            (define mid(lambda(seq)(/ (length seq) 2)))
                                ((combine append) (take(mid deck) deck) (drop(mid deck) deck)))))"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("(riff-shuffle (list 1 2 3 4 5 6 7 8))"));
            Assert.AreEqual(res.Type, Cell.CellType.List);
            Assert.AreEqual(res.list.Count, 8);
            Assert.AreEqual(res.list[0].number, 1f);
            Assert.AreEqual(res.list[1].number, 5f);
            Assert.AreEqual(res.list[2].number, 2f);
            Assert.AreEqual(res.list[3].number, 6f);
            Assert.AreEqual(res.list[4].number, 3f);
            Assert.AreEqual(res.list[5].number, 7f);
            Assert.AreEqual(res.list[6].number, 4f);
            Assert.AreEqual(res.list[7].number, 8f);

            res = Program.Eval(Program.Parse("(define compose (lambda (f g) (lambda (x) (f (g x)))))"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("(define repeat (lambda (f) (compose f f)))"));
            Assert.AreEqual(res, null);

            res = Program.Eval(Program.Parse("((repeat riff-shuffle) (list 1 2 3 4 5 6 7 8))"));
            Assert.AreEqual(res.Type, Cell.CellType.List);
            Assert.AreEqual(res.list.Count, 8);
            Assert.AreEqual(res.list[0].number, 1f);
            Assert.AreEqual(res.list[1].number, 3f);
            Assert.AreEqual(res.list[2].number, 5f);
            Assert.AreEqual(res.list[3].number, 7f);
            Assert.AreEqual(res.list[4].number, 2f);
            Assert.AreEqual(res.list[5].number, 4f);
            Assert.AreEqual(res.list[6].number, 6f);
            Assert.AreEqual(res.list[7].number, 8f);

            res = Program.Eval(Program.Parse("(riff-shuffle (riff-shuffle (riff-shuffle (list 1 2 3 4 5 6 7 8))))"));
            Assert.AreEqual(res.Type, Cell.CellType.List);
            Assert.AreEqual(res.list.Count, 8);
            Assert.AreEqual(res.list[0].number, 1f);
            Assert.AreEqual(res.list[1].number, 2f);
            Assert.AreEqual(res.list[2].number, 3f);
            Assert.AreEqual(res.list[3].number, 4f);
            Assert.AreEqual(res.list[4].number, 5f);
            Assert.AreEqual(res.list[5].number, 6f);
            Assert.AreEqual(res.list[6].number, 7f);
            Assert.AreEqual(res.list[7].number, 8f);
        }
        
    }
}
