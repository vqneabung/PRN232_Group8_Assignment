# Batch Grading API - H??ng D?n S? D?ng

## T?ng Quan
API này cho phép ch?m bài hàng lo?t t? m?t file ZIP/RAR/7Z ch?a nhi?u bài làm c?a sinh viên.

**H? tr? các ??nh d?ng file nén:**
- ? `.zip` - ZIP archives
- ? `.rar` - RAR archives  
- ? `.7z` - 7-Zip archives

## C?u Trúc File Yêu C?u

```
PRN232_SU25_PE_Block10w_PhuongLHK_(SE1751).rar
?
??? PRN232_SU25_PE_Block10w_PhuongLHK_(SE1751)/
  ??? AnhNASE183208/    ? Th? m?c sinh viên
    ?   ??? history.dat
    ? ??? 0/
    ?   ??? solution.zip     ? Bài làm c?a sinh viên
    ?
    ??? DuyPNSE173520/
    ?   ??? history.dat
?   ??? 0/
    ?       ??? solution.zip
    ?
    ??? [Các th? m?c sinh viên khác]/
  ??? 0/
            ??? solution.zip
```

**L?u ý quan tr?ng:**
- File ngoài cùng có th? là `.zip`, `.rar`, ho?c `.7z`
- Bên trong, file bài làm c?a sinh viên (solution.zip) v?n ph?i là file `.zip`
- H? th?ng t? ??ng nh?n di?n và extract ?úng ??nh d?ng file

## Gi?i H?n

- Maximum file size: **500MB**
- Supported formats: `.zip`, `.rar`, `.7z`
- Timeout: Tùy thu?c vào s? l??ng sinh viên (khuy?n ngh? < 100 sinh viên/l?n)

## Dependencies

API s? d?ng các th? vi?n sau ?? x? lý file nén:
- **System.IO.Compression** - X? lý ZIP files (built-in .NET)
- **SharpCompress** - X? lý RAR, 7Z và các ??nh d?ng khác

## Các Tr??ng H?p ??c Bi?t

## Troubleshooting

### "End of Central Directory record could not be found"
**Nguyên nhân**: File là RAR nh?ng code c? ch? h? tr? ZIP
**Gi?i pháp**: ? ?ã fixed - H? th?ng hi?n h? tr? ZIP, RAR, và 7Z

### "Could not extract class code from filename"
**Nguyên nhân**: Tên file không ch?a mã l?p trong d?u ngo?c ??n
**Gi?i pháp**: ??i tên file thành format: `filename_(CLASSCODE).ext`

````````

# Response
````````markdown
