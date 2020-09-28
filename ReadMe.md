### Veeam.Gzipper 
![Veeam](veeam.png)


�������� ������ ��� ������ ������. 

> ����������: ������������ ������ � ������� ������ �������� ( ����� ��� WinRAR ) �� ������ ���������� ���������, ��� ��� ����� ������������ ������������ ������ �������� �������� � �������� ���� ������ ��� ���� �������� ����������. 

#### �������������

    GZipTest.exe [ compress / decompress] [ ����. ���� / ���. ���� ] [ ���. ���� / ����. ���� ]
��� ������ ��������� ����� ���������� ��� ��� ����� ����������. ��� ���������� ���������� ��������� �������� ������ ������� ������ � ������ �������. ������ ��������� ����� ������������. 

#### ��� ��������?
**���������** - ��� ������ ��������� ���������� �������� ������ ������ �� ������ ����� �����, ��� ���� � ������ ������ �������� ��������������� ������. ������ 8 ���� ��������� ����� �������� *������ ������������� �����*. ������ 8 ���� �������� *������ ��������� ����������� ������* ��� ���������. ��� ������������ ��� ����, ����� ��� ��������� �������� ��������� ( ������� ��������� ������ ) ��� ������������ ��������� ������ ��������� ������ ������ ������, ������� ���� ��������� �� ����� ��������� �� ������� �����������. ������ � ����� ������ ������������� ���������, �� �� ��������� ��������� ������ ������� ���������� ����� `GZipStream`. 
**������������** - ��� ������������ ������ ������ ���������� ���������������. ������ �� ��������� ����������� �����������. � ������� �������� ����������� ��� ��������� ����� ��������� ����������� ������� ������ � ���������� � ����� � ��������������� �������. 

#### ������������������. ���������� � WinRAR ( ������� ������� ������ ����� ).
|  | GZipper | WinRAR |
|--|--|--|
| �� ���������  | 2 628 741 �� |	2 628 741 �� |
| ����� ��������� | 2 605 447 �� |	2 607 705 �� |
| ����� ��������� | 1� 35� |3� 53�	 |
| ������ | 21-23 �� |	420-440 �� |
| ��������� | 34-40% |	95-97% |

� ��� �������� �����������. ������� ���� ���-��� ������ WinRAR ����� ������� ��������. 

#### ����� ����������� �������
[Veeam.Gzipper.Core](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Core) - �������� ��������, ���������-����������� ����������. 
[Veeam.Gzipper.Cmd](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Cmd) - ���������� ����������� [Veeam.Gzipper.Core](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Core)  ��� ���������� ����������. 
[Veeam.Gzipper.Tests](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Tests) - ����-����� ��� ������� ���������� [Veeam.Gzipper.Core](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Core).
[GZipTest](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/GZipTest) - ���������� ���������� ��� ������������� � ��-������������� ������. ���������� ���������� ������� [Veeam.Gzipper.Core](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Core) � [Veeam.Gzipper.Cmd](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Cmd). 

#### ��������� ���������� ������� � �������� �������� ������� 
[SyncLimitedThreads](https://github.com/grigdevelop/Veeam.Gzipper/blob/master/src/Veeam.Gzipper.Core/Threads/Limitations/SyncLimitedThreads.cs) - �������� ����� ��� ������� ������ � ��������. ��������� ���������� ��������� ��������� �������, � ����� �������������� ���������� �������� �������. ��� ����������� ������ �� ��������� ������� ���������� � ����������� ������, �������� ������ �������� ������ *Interrupt* ��� ������ �� ������ � ������ ������ [StlInterrupt](https://github.com/grigdevelop/Veeam.Gzipper/blob/master/src/Veeam.Gzipper.Core/Threads/Limitations/StlInterrupt.cs), � ������ ������� � �������� ����� �� ����������. 
[StreamChunkReader](https://github.com/grigdevelop/Veeam.Gzipper/blob/master/src/Veeam.Gzipper.Core/Streams/StreamChunkReader.cs) - ����� ��� ������ ������ �� ������ ������� �������� �������� �� ������ ������� � ������������� ������. 
[ChunkedStreamReader](https://github.com/grigdevelop/Veeam.Gzipper/blob/master/src/Veeam.Gzipper.Core/Streams/ChunkedStreamReader.cs) - ����� ��� ��� ����������������� ������ � ������������� ��������� ������ � ���� ������ �������� �������� �� ������ ����� ����������� � ������� ������ [StreamChunkReader](https://github.com/grigdevelop/Veeam.Gzipper/blob/master/src/Veeam.Gzipper.Core/Streams/StreamChunkReader.cs). 


#### �����
���������� [�����](https://github.com/grigdevelop/Veeam.Gzipper/tree/master/src/Veeam.Gzipper.Tests) �������� ����� ���� ������������ ��� ������� ��� ����������� ���������� ������ �������. 


#### ���������� ( Exception )
������ ����� ������ �� ����������� ��������� ���������� � ������� ��������������� ����� � �������������� ��, �� ��������� �������� ����������, ����� ��� "���� �� ������", "���� ������������ ������ ���������" � �. �. ��������� �������� ���� ����-�������, �� ����� ������� ������ � �������� ������, ������������� ���� ����������. 