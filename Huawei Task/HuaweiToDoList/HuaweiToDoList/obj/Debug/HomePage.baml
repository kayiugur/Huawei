   M S B A M L   `   `   `  ����I  EHuaweiToDoList, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:HuaweiToDoListHuaweiToDoList  5   	       HuaweiToDoList.HomePage   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:HuaweiToDoList     5   	   -   5          � Title$  	Home Page��5   	   $	��450��6   $	��800��6(    � Icon$ 1-download.png��64   $	��800��6a   $	��450��6p   $	��800��6�   $	��450��6�   .��5	      � ��5
   
   �� 5       �� ImageSource$ 3-image.png��6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ dataGridShow��5      $
��Left=�6)   $	��233��6D   $��114,128,0,0q�6Q   $	��Top=�6f   $	��380��6~   $��MV Boli)�6�   
�������$	��384��6�   $	��233��6�   6�   �� 5   
   -   5         ��Name$ label��5      $��Search��6   $
��Left=�60   $��
114,90,0,0q�6K   $	��Top=�6_   $��MV Boli)�6w   $��63��6�   �� 5   
   -   5         ��Name$ 	SearchBox��5      $
��Left=�6%   $��18��6@   $��
165,95,0,0q�6L    �� TextWrapping$
 Wrap=�6`   $	��Top=�6t   $	��230��6�   $��MV Boli)�6�    �� 	UndoLimit$	 250��6�   �� 5   
   -   5         ��Name$ InsertButton��5      $��Insert��6'   $
��Left=�68   $��524,319,0,0q�6S   $	��Top=�6h   $��99��6�   $��MV Boli)�6�   6�   �� 5   
   -   5        $ UpdateButton��5      $��Update��6'   $
��Left=�68   $��524,346,0,0q�6S   $	��Top=�6h   $��99��6�   $��MV Boli)�6�   6�   �� 5   
   -   5        $ DeleteButton��5      $��Delete��6'   $
��Left=�68   $��524,373,0,0q�6S   $	��Top=�6h   $��99��6�   $��MV Boli)�6�   6�   �� 5   
   -   5        $ SearchButton��5      $��Search��6'   $
��Left=�68   $��
413,94,0,0q�6S   $	��Top=�6g   $��85��6   $��MV Boli)�6�   6�   �� 5   
   -	   5        $ 
ExitButton��5      $
��Exit��6%   $
��Left=�64   $��699,388,0,0q�6O   $	��Top=�6d   $��85��6|   $��MV Boli)�6�   6�   �� 5   
   -
   5        $ UserInfo��5      $
��Left=�6$   $��23��6?   $��
632,10,0,0q�6K   $
 Wrap=�6_   $	��Top=�6s   $	��125��6�   $��MV Boli)�6�   $
��Bold"�6�   .��6�   ��5      �� 5      .��5   #   ��5      �� 5      .��5   "   5      �� 5   
   -   5        $
 User��5      $��User :��6   $
��Left=�6/   $��	587,7,0,0q�6J   $	��Top=�6]   $��45��6u   $��MV Boli)�6�   $
��Bold"�6�   �� 5   
   -   5        $ TodoText��5      $
��Left=�6$   $	��123��6?   $��524,147,0,0q�6L   $
 Wrap=�6a   $	��Top=�6u   $	��222��6�   $��MV Boli)�6�   (  "System.Windows.Controls.DatePicker  5   
   -   5        	  Name$	 DueDate��5      $
��Left=�6&   $��614,275,0,0q�6A   $	��Top=�6V   $	��132��6n   $��MV Boli)�6z   6�   �� 5    
   -   5        $ label1��5       $��
Due Date :��6    $
��Left=�65   $��535,274,0,0q�6P   $	��Top=�6e   $��74��6}   $��MV Boli)�6�   �� 5!   
   -   5        $ label2��5!      $��To Do��6    $
��Left=�60   $��586,128,0,0q�6K   $	��Top=�6`   $��MV Boli)�6x   $��46��6�   5#      5$      