   M S B A M L   `   `   `  ����   SHOOTER_MESSANGER+    %SHOOTER_MESSANGER.SearchFriendsWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  	Messenger��$	��600��$	��900�� � 
ResizeMode$ CanResizeWithGrip=�.��� $��10q�.+��� .@��� $��Друзья��.��� $��10q� � RowDefinitions �� $��* ��� $
��Auto �.+��� -   $��FriendsListBox��$��0��$��0,0,0,10q����� .R���  �� Orientation$ 
Horizontal=�$	��0,5q�.+��� $
��Bold"����� *
FriendName+�� -   $��Удалить друга��$��10,0,0,0q�
 � Tag �� *
FriendId+�� -   $��FriendsMessageBox��$��1��$��35�� �� 
IsReadOnly . $��0,10,0,0q��� $��Заявки��.��� $��10q� �� $
��Auto ��� $
��Auto ��� $��* �.+��� $#��Входящие заявки��$��0,0,0,10q�$��0��.���� -   $��IncomingRequestsListBox������ .R��� $ 
Horizontal=�$	��0,5q�.+��� $
��Bold"����� *RequesterName+�� -   $��Принять��$��10,0,0,0q� �� *RequesterId+�� -   $��Отклонить��$��5,0,0,0q� �� *RequesterId+�� $%��Исходящие заявки��$��0,0,0,10q�$��1��.���� -   $��OutgoingRequestsListBox������ .R��� $ 
Horizontal=�$	��0,5q�.+��� $
��Bold"����� *ReceiverName+�� $2��, | Ожидает подтверждения���� $��
Поиск��.��� $��10q� �� $
��Auto ��� $��* ��� $
��Auto � � ColumnDefinitions �� $	��63* ��� $
��691* ��� $
��Auto �.+��� -   $��	SearchBox��$��35��$��Center=�$��0��$��0,0,10,0q�$��2���� -	   $��SearchButton��$��
Найти��$��35��$	��100��$��Center=�$��Center=�$��2���� -
   $��SearchResultsListBox��$��1��$��3��$��	0,10,0,10q����� .R��� $ 
Horizontal=�$	��0,5q�.+��� $
��Bold"����� *
FriendName+�� -   $&�� Добавить в друзья��$��10,0,0,0q� �� *
FriendId+�� -   $��MessageBoxText��$��2��$��35�� . $��0,10,0,0q�$��2���� -   $��Чаты��$��Right=�$	��Top=�$��	0,0,10,10q�$
��10,5q�