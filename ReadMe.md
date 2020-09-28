### Veeam.Gzipper 
![Veeam](veeam.png)


Тестовый проект для сжатия файлов. 

> Примечание: Разархивация файлов с помощью других программ ( таких как WinRAR ) не выдаст корректный результат, так как чтобы многопоточно обрабатывать данные пришлось добавить в архивный файл чуждые для этих программ метаданные. 

#### Использование

    GZipTest.exe [ compress / decompress] [ ориг. файл / арх. файл ] [ арх. файл / ориг. файл ]
При вызове программы можно пропустить все или часть параметров. При отсутствии параметров программа запросит нужные вводные данные в режиме запуска. Лишние параметры будут игнорированы. 

#### Как работает?
**Архивация** - При сжатии программа асинхронно начинает читать данные из разных точек файла, при этом в каждом потоке сохраняя соответствующий индекс. Первые 8 байт архивного файла занимает *размер оригинального файла*. Вторые 8 байт занимает *размер доступной оперативной памяти* при архивации. Она используется для того, чтобы при изменении настроек программы ( размера доступной памяти ) при разархивации программа смогла вычислять размер кусков потока, которая была применена во время архивации со старыми настройками. Запись в поток архива производиться синхронно, из за специфики алгоритма сжатия который использует класс `GZipStream`. 
**Разархивация** - При разархивации чтение данных происходит последовательно. Однако их обработка выполняется параллельно. С помощью индексов сохраненных при архивации метод вычисляет изначальную позицию данных и записывает в поток в соответствующей позиции. 

#### Производительность. Сравниваем с WinRAR ( средний уровень сжатие файла ).
|  | GZipper | WinRAR |
|--|--|--|
| До архивации  | 2 628 741 КБ |	2 628 741 КБ |
| После архивации | 2 605 447 КБ |	2 607 705 КБ |
| Время архивации | 1м 35с |3м 53с	 |
| Помять | 21-23 МБ |	420-440 МБ |
| Процессор | 34-40% |	95-97% |

Я сам удивился результатам. Наверно надо все-так купить WinRAR чтобы быстрее работала. 

#### Общая архитектура проекта
[Veeam.Gzipper.Core](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Core) - Содержит основной, платформа-независимый функционал. 
[Veeam.Gzipper.Cmd](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Cmd) - Реализация интерфейсов [Veeam.Gzipper.Core](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Core)  для консольных приложений. 
[Veeam.Gzipper.Tests](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Tests) - Юнит-тесты для классов библиотеки [Veeam.Gzipper.Core](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Core).
[GZipTest](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/GZipTest) - Консольное приложения для архивирования и де-архивирования файлов. Использует библиотеки классов [Veeam.Gzipper.Core](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Core) и [Veeam.Gzipper.Cmd](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Cmd). 

#### Структура реализации проекта и описание основных классов 
[SyncLimitedThreads](https://github.com/grigdevelop/Veeam.Gzipper/blob/master/src/Veeam.Gzipper.Core/Threads/Limitations/SyncLimitedThreads.cs) - основной класс для решения задачи с потоками. Позволяет асинхронно запускать несколько потоков, а также контролировать количество активных потоков. При исключениях ошибки из созданных потоков передаются к вызывающему потоку, активные потоки получают запрос *Interrupt* для выхода из метода с помощи класса [StlInterrupt](https://github.com/grigdevelop/Veeam.Gzipper/blob/master/src/Veeam.Gzipper.Core/Threads/Limitations/StlInterrupt.cs), а потоки которые в ожидании далее не вызываться. 
[StreamChunkReader](https://github.com/grigdevelop/Veeam.Gzipper/blob/master/src/Veeam.Gzipper.Core/Streams/StreamChunkReader.cs) - класс для чтения данных из стрима кусками байтовых массивов из разных позиций в многопоточном режиме. 
[ChunkedStreamReader](https://github.com/grigdevelop/Veeam.Gzipper/blob/master/src/Veeam.Gzipper.Core/Streams/ChunkedStreamReader.cs) - класс для для последовательного чтения и многопоточной обработки данных в виде кусков байтовых массивов из стрима ранее записанного с помощью класса [StreamChunkReader](https://github.com/grigdevelop/Veeam.Gzipper/blob/master/src/Veeam.Gzipper.Core/Streams/StreamChunkReader.cs). 


#### Тесты
Написанные [тесты](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Tests) являются всего лишь экземплярами или основой для дальнейшего дополнения новыми кейсами. 


#### Исключения ( Exception )
Скорее всего стоило бы захватывать отдельные исключения с помощью соответствующих типов и переопределять их, но сообщения основных исключений, таких как "файл не найден", "файл используется другим процессом" и т. д. выглядели довольно таки юзер-френдли, по этому оставил только с захватом общего, родительского типа исключения. 