mkdir transparent
for %%G in (*.bmp) do (
"C:\Program Files\ImageMagick-7.0.7-Q16\magick.exe" "%%G" -transparent #61442b "transparent\%%G"
)
cd transparent
"C:\Program Files\ImageMagick-7.0.7-Q16\magick.exe" mogrify -format png *.bmp
del /s /q /f *.bmp
pause