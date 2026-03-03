import ShowPdfOnly from "./ShowPdfOnly";
import ShowWordOnly from "./ShowWordOnly";
import ShowExcelOnly from "./ShowExcelOnly";
import ShowImg from "./ShowImg";
import ShowTextOnly from "./ShowTextOnly";
import ShowPptOnly from "./ShowPptOnly";

export default function Index({ open, setOpen, file }: any) {
  const fileType = file.documentType;
  const documentFile = file;

  return (
    <>
      {fileType === ".pdf" && (
        <ShowPdfOnly open={open} setOpen={setOpen} file={documentFile} />
      )}
      {(fileType === ".docx" || fileType === ".doc") && (
        <ShowWordOnly open={open} setOpen={setOpen} file={documentFile} />
      )}
      {(fileType === ".pptx" || fileType === ".ppt") && (
        <ShowPptOnly open={open} setOpen={setOpen} file={documentFile} />
      )}
      {(fileType === ".xlsx" || fileType === ".xlx" || fileType === ".xls") && (
        <ShowExcelOnly open={open} setOpen={setOpen} file={documentFile} />
      )}
      {(fileType === ".png" || fileType === ".jpg" || fileType === ".jpeg") && (
        <ShowImg open={open} setOpen={setOpen} file={documentFile} />
      )}
      {fileType === ".txt" && (
        <ShowTextOnly open={open} setOpen={setOpen} file={documentFile} />
      )}
    </>
  );
}
