// routes/PreviewPage.tsx
import { useLocation } from "react-router-dom";
// import { PdfViewerComponent } from "@syncfusion/ej2-react-pdfviewer";
// import { registerLicense } from '@syncfusion/ej2-base';
// registerLicense('Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtdcHVVQ2FZWUBxXkFWYUA=');

const PreviewPage = () => {
  const location = useLocation();
  const blobUrl = location.state?.blobUrl;
  console.log(blobUrl)

  if (!blobUrl) return <div className="text-red-600">Tidak ada file untuk ditampilkan.</div>;

  return (
    <div className="w-full">
      <h2 className="text-xl font-bold mb-4">Preview Hasil Edit PDF</h2>
      <iframe src={blobUrl} />
    </div>
  );
};

export default PreviewPage;
