   M S B A M L   `   `   `  ����  SHOOTER_MESSANGER+    %SHOOTER_MESSANGER.SearchFriendsWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Поиск друзей��5   	   $	��500��6   $	��800��6+    � 
ResizeMode$ CanResizeWithGrip=�67   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5
      $��* �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� -   5        $��	SearchBox��5      $��35��6#   $��Center=�5      $��Stretch=�5      $��0��5      $��0��6   $��0,0,10,0q�5      �� 5!   
   -   5        $��SearchButton��5!      $��
Найти��6%   $��35��5"      $	��100��6   $��Center=�5#      $��Right=�5$      $��0��5%      $��1��6   5&      �� 5)   
   -   5        $��SearchResultsListBox��5)      $��1��5*      $��2��6   $��Stretch=�5+      $��	0,10,0,10q�6.   ��5,      �� 5-      .R�5.      ��  �� Orientation$ 
Horizontal=�6!   $	��0,5q�6:   .+�5/      �� $
��Bold"�6@   ��6$   �� *
FriendName+6@   �� 50      $�� | ID: ��6$   �� 51      ��6$   �� *
FriendId+�� 52      -   5        $&�� Добавить в друзья��52   !   $��10,0,0,0q�55   !   
 � Tag 54   !   �� *
FriendId+55   !   56      57      59      �� 5<   
   -   5        $��MessageBoxText��5<      $��35��6(   $	��Top=�5=      $��Stretch=�5>      $��2��5?      $��2��6    �� 
IsReadOnly . $��0,10,0,0q�5A      5B      5C      