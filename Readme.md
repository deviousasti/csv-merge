
#CSV Merge

Basic Usage
-----------

![Main window](https://user-images.githubusercontent.com/2375486/65368444-a97ba500-dc5e-11e9-96a0-40143940efe2.png)

Add files by dragging and dropping them into the window.
You can also drag in whole folders and they will be recursively searched for csv files. This is useful when the actual files are structured like 'Project/Outputs/BOM.csv'.

You can delete files by selecting entries from the list and hitting the delete key. You can select all by dragging or using `Ctrl+A`. 

Drag and drop multiple times in case you missed something. Files won't be duplicated.

Merging
-------

![Files added](https://user-images.githubusercontent.com/2375486/65368762-e5186e00-dc62-11e9-8fab-311041e63074.png)

Once the files are added, just hit merge. 

Some files may have a different number of headers, or the columns may be in a different order. Select one of the entries to choose which file's column order to use as output.

**Note**

The merged output will have a column inserted named (Source) which will contain the name of the file that contained the row.


Consolidation
--------------

As with merge, select one of the files to choose a column order.

![Consolidation screen](https://user-images.githubusercontent.com/2375486/65369354-86a2be00-dc69-11e9-9bbf-6da6978e219f.png)

From the list choose one or more columns to *act as a key*.
The combination of key fields will be used to determine the number of rows.

Click on one of the `Combine` fields to choose the type of operation.

###Combine operations

####`Union` 
Set union, which discards duplicates
`A, A, B, C, A, B, D` becomes `A, B, C, D`

####`Join` 
Joins all fields with the specified separator - if none is specified, a semicolon `;` is used

####`Total` 
Sums all the fields together. A numeric type is expected. 

--------------------------------------------------------------------------

Example
-------


| Sales_Rep_ID | Postcode | Sales_Rep_Name | Year | Value   | Commission |
| ------------ | -------- | -------------- | ---- | ------- | ---------- |
| 456          | 2027     | Jane           | 2017 | 11,444  | 5%         |
| 456          | 2110     | Jane           | 2017 | 30,569  | 5%         |
| 123          | 2137     | John           | 2017 | 83,085  | 5%         |
| 789          | 2164     | Jack           | 2017 | 82,490  | 5%         |
| 789          | 2067     | Jack           | 2018 | 23,819  | 5%         |


1. Choosing `Sales_Rep_ID` as the *key*, the consolidated result will have only `456, 123, 789`.

| Sales_Rep_ID | Postcode   | Sales_Rep_Name | Year       | Value          | Commission |
| ------------ | ---------- | -------------- | ---------- | -------------- | ---------- |
| 456          | 2027, 2110 | Jane           | 2017       | 11,444, 30,569 | 5%         |
| 123          | 2137       | John           | 2017       | 83,085         | 5%         |
| 789          | 2164, 2067 | Jack           | 2017, 2018 | 82,490, 23,819 | 5%         |

2. Choosing `Sales_Rep_ID` and `Year` as the key:

| Sales_Rep_ID | Postcode   | Sales_Rep_Name | Year | Value          | Commission |
| ------------ | ---------- | -------------- | ---- | -------------- | ---------- |
| 456          | 2027, 2110 | Jane           | 2017 | 11,444, 30,569 | 5%         |
| 123          | 2137       | John           | 2017 | 83,085         | 5%         |
| 789          | 2164       | Jack           | 2017 | 82,490         | 5%         |
| 789          | 2067       | Jack           | 2018 | 23,819         | 5%         |

3. Choosing `Year` as the key, and *Total* on `Value`:

| Sales_Rep_ID  | Postcode               | Sales_Rep_Name   | Year | Value  | Commission |
| ------------- | ---------------------- | ---------------- | ---- | ------ | ---------- |
| 456, 123, 789 | 2027, 2110, 2137, 2164 | Jane, John, Jack | 2017 | 207588 | 5%         |
| 789           | 2067                   | Jack             | 2018 | 23819  | 5%         |


4. Choosing `Year` as the key, and *Total* on `Value`:

| Sales_Rep_ID  | Postcode               | Sales_Rep_Name   | Year | Value  | Commission |
| ------------- | ---------------------- | ---------------- | ---- | ------ | ---------- |
| 456, 123, 789 | 2027, 2110, 2137, 2164 | Jane, John, Jack | 2017 | 207588 | 5%         |
| 789           | 2067                   | Jack             | 2018 | 23819  | 5%         |

5. Choosing `Year` as the key, *Total* on `Value`, and *Join* on `Sales_Rep_ID` and `Sales_Rep_Name`:

![Setting screen](https://user-images.githubusercontent.com/2375486/65372156-e14c1200-dc89-11e9-85fd-2a08fa7d3807.png)

|Sales_Rep_ID       | Postcode               | Sales_Rep_Name         | Year | Value  | Commission |
|------------------ | ---------------------- | ---------------------- | ---- | ------ | ---------- |
|456; 456; 123; 789 | 2027, 2110, 2137, 2164 | Jane; Jane; John; Jack | 2017 | 207588 | 5%         |
|789                | 2067                   | Jack                   | 2018 | 23819  | 5%         |


