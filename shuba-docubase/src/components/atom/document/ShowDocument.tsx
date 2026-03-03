import ShowPdf from "./ShowPdf";
import ShowWord from "./ShowWord";
import ShowExcel from "./ShowExcel";
import ShowImg from "./ShowImg";
import ShowText from "./ShowText";
import ShowPptOnly from "./ShowPptOnly";

export default function Index({ open, setOpen, file }: any) {
  const fileType = file.documentType;
  const documentFile = file;

  return (
    <>
      {fileType === ".pdf" && (
        <ShowPdf open={open} setOpen={setOpen} file={documentFile} />
      )}
      {(fileType === ".docx" || fileType === ".doc") && (
        <ShowWord open={open} setOpen={setOpen} file={documentFile} />
      )}
      {(fileType === ".pptx" || fileType === ".ppt") && (
        <ShowPptOnly open={open} setOpen={setOpen} file={documentFile} />
      )}
      {(fileType === ".xlsx" || fileType === ".xlx" || fileType === ".xls") && (
        <ShowExcel open={open} setOpen={setOpen} file={documentFile} />
      )}
      {(fileType === ".png" || fileType === ".jpg" || fileType === ".jpeg") && (
        <ShowImg open={open} setOpen={setOpen} file={documentFile} />
      )}
      {fileType === ".txt" && (
        <ShowText open={open} setOpen={setOpen} file={documentFile} />
      )}
    </>
  );
}
