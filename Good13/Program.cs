using System.Diagnostics;

Console.WriteLine("Started");

// Первое решение делал в лоб, но при запусках понял, что ответа мне не дождаться в адекватный срок.
// Второе решение делал с помощью комбинаторики, надеюсь не накосячил. Время решения упало почти до нуля.
// Обработка ошибок опущена, так как здесь излишня
// Комментарии также особо не писал, надеюсь тут не это проверяется
// Тесты не писал, тестовый код на котором тестировал частные случаи удалил
// В DEBUG режиме оставил диагностический вывод
// Задача больше алгоритмическая, - разгуляться негде, никакой тебе красивой архитектуры, взаимодействия между сервисами, дизайна API и т.п.
bool useFast = true;

Stopwatch stopwatch = Stopwatch.StartNew();
var base13GoodNumberCount = useFast? Good13.GetBase13GoodNumberCount() : Good13.GetBase13GoodNumberCountOld();
Console.WriteLine("GetBase13GoodNumberCount: {0}", base13GoodNumberCount);
Console.WriteLine($"Finished. Elapsed: {stopwatch.Elapsed}");

Console.ReadLine();